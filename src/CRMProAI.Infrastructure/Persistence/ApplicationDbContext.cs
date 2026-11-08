using System.Linq.Expressions;
using CRMProAI.Application.Common;
using CRMProAI.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CRMProAI.Infrastructure.Persistence;

/// <summary>
/// Core database context providing PostgreSQL conventions, multi-tenant isolation,
/// soft-deletion query filters, UTC timestamp guarantees, and automatic audit metadata stamping.
/// </summary>
public class ApplicationDbContext : DbContext
{
    private readonly ITenantContext? _tenantContext;
    private readonly ICurrentUserContext? _currentUserContext;

    public Guid CurrentTenantId => _tenantContext?.TenantId ?? Guid.Empty;
    public bool HasTenant => _tenantContext?.HasTenant ?? false;

    public DbSet<CRMProAI.Domain.Entities.BaselineProbe> BaselineProbes => Set<CRMProAI.Domain.Entities.BaselineProbe>();
    public DbSet<CRMProAI.Domain.Modules.Tenancy.Tenant> Tenants => Set<CRMProAI.Domain.Modules.Tenancy.Tenant>();

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantContext? tenantContext = null,
        ICurrentUserContext? currentUserContext = null)
        : base(options)
    {
        _tenantContext = tenantContext;
        _currentUserContext = currentUserContext;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // PostgreSQL extensions
        modelBuilder.HasPostgresExtension("uuid-ossp");
        modelBuilder.HasPostgresExtension("pg_trgm");

        // Apply entity configurations from Infrastructure assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Apply global conventions and query filters
        ConfigureUtcDateTimeConventions(modelBuilder);
        ConfigureGlobalQueryFilters(modelBuilder);
    }

    private static void ConfigureUtcDateTimeConventions(ModelBuilder modelBuilder)
    {
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var dateTimeNullableConverter = new ValueConverter<DateTime?, DateTime?>(
            v => !v.HasValue ? null : (v.Value.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)),
            v => !v.HasValue ? null : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc));

        var dateTimeOffsetConverter = new ValueConverter<DateTimeOffset, DateTimeOffset>(
            v => v.ToUniversalTime(),
            v => v.ToUniversalTime());

        var dateTimeOffsetNullableConverter = new ValueConverter<DateTimeOffset?, DateTimeOffset?>(
            v => !v.HasValue ? null : v.Value.ToUniversalTime(),
            v => !v.HasValue ? null : v.Value.ToUniversalTime());

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(dateTimeNullableConverter);
                }
                else if (property.ClrType == typeof(DateTimeOffset))
                {
                    property.SetValueConverter(dateTimeOffsetConverter);
                }
                else if (property.ClrType == typeof(DateTimeOffset?))
                {
                    property.SetValueConverter(dateTimeOffsetNullableConverter);
                }
            }
        }
    }

    private void ConfigureGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            var isTenantScoped = typeof(ITenantScoped).IsAssignableFrom(clrType);
            var isSoftDeletable = typeof(ISoftDeletable).IsAssignableFrom(clrType);

            if (!isTenantScoped && !isSoftDeletable)
            {
                continue;
            }

            var parameter = Expression.Parameter(clrType, "e");
            Expression? filterExpression = null;

            if (isSoftDeletable)
            {
                // Expression: !e.IsArchived
                var isArchivedProp = Expression.Property(parameter, nameof(ISoftDeletable.IsArchived));
                var notArchived = Expression.Equal(isArchivedProp, Expression.Constant(false));
                filterExpression = notArchived;
            }

            if (isTenantScoped)
            {
                // Expression: e.TenantId == CurrentTenantId
                var tenantIdProp = Expression.Property(parameter, nameof(ITenantScoped.TenantId));
                var currentTenantIdExpr = Expression.Property(Expression.Constant(this), nameof(CurrentTenantId));
                var tenantMatch = Expression.Equal(tenantIdProp, currentTenantIdExpr);

                filterExpression = filterExpression == null
                    ? tenantMatch
                    : Expression.AndAlso(filterExpression, tenantMatch);
            }

            if (filterExpression != null)
            {
                var lambda = Expression.Lambda(filterExpression, parameter);
                modelBuilder.Entity(clrType).HasQueryFilter(lambda);
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var currentUserId = _currentUserContext?.UserId;

        // Process Auditing
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    SetProperty(entry, nameof(IAuditableEntity.CreatedAtUtc), now);
                    if (currentUserId.HasValue)
                    {
                        SetProperty(entry, nameof(IAuditableEntity.CreatedByUserId), currentUserId);
                    }
                    break;

                case EntityState.Modified:
                    SetProperty(entry, nameof(IAuditableEntity.UpdatedAtUtc), now);
                    if (currentUserId.HasValue)
                    {
                        SetProperty(entry, nameof(IAuditableEntity.UpdatedByUserId), currentUserId);
                    }
                    break;
            }
        }

        // Process Soft Deletes
        foreach (var entry in ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                SetProperty(entry, nameof(ISoftDeletable.IsArchived), true);
                SetProperty(entry, nameof(ISoftDeletable.ArchivedAtUtc), now);
                if (currentUserId.HasValue)
                {
                    SetProperty(entry, nameof(ISoftDeletable.ArchivedByUserId), currentUserId);
                }
            }
        }

        // Process Tenant Scoping on Added entities
        if (HasTenant)
        {
            foreach (var entry in ChangeTracker.Entries<ITenantScoped>())
            {
                if (entry.State == EntityState.Added)
                {
                    var currentTenantValue = (Guid)entry.Property(nameof(ITenantScoped.TenantId)).CurrentValue!;
                    if (currentTenantValue == Guid.Empty)
                    {
                        entry.Property(nameof(ITenantScoped.TenantId)).CurrentValue = CurrentTenantId;
                    }
                }
            }
        }

        // Collect domain events before saving
        var domainEntities = ChangeTracker.Entries<AggregateRoot<Guid>>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Clear domain events after successful persistence
        foreach (var entity in domainEntities)
        {
            entity.ClearDomainEvents();
        }

        return result;
    }

    private static void SetProperty(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, string propertyName, object? value)
    {
        var property = entry.Property(propertyName);
        if (property.Metadata.PropertyInfo?.CanWrite == true)
        {
            property.CurrentValue = value;
        }
    }
}
