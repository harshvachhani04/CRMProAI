using System.Linq.Expressions;
using CRMProAI.Application.Common;
using CRMProAI.Domain.Common;
using CRMProAI.Domain.Entities;
using CRMProAI.Domain.Exceptions;
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
    public bool FilterTenantScopedEntities { get; private set; } = true;

    public DbSet<BaselineProbe> BaselineProbes => Set<BaselineProbe>();
    public DbSet<TenantProbe> TenantProbes => Set<TenantProbe>();
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

    /// <summary>
    /// Creates a disposable scope that temporarily bypasses tenant query filters and mutation guards,
    /// enabling platform-level administrative, background processing, or cross-tenant tasks.
    /// Soft-delete and other domain filters remain active.
    /// </summary>
    public IDisposable CreateTenantFilterBypassScope()
    {
        return new TenantFilterBypassScope(this);
    }

    private sealed class TenantFilterBypassScope : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly bool _previousState;

        public TenantFilterBypassScope(ApplicationDbContext context)
        {
            _context = context;
            _previousState = context.FilterTenantScopedEntities;
            _context.FilterTenantScopedEntities = false;
        }

        public void Dispose()
        {
            _context.FilterTenantScopedEntities = _previousState;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // PostgreSQL extensions
        modelBuilder.HasPostgresExtension("uuid-ossp");
        modelBuilder.HasPostgresExtension("pg_trgm");

        // Apply entity configurations from Infrastructure assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Enforce non-nullable TenantId and index on all ITenantScoped entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(ITenantScoped.TenantId))
                    .IsRequired();

                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(ITenantScoped.TenantId));
            }
        }

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
                // Expression: !FilterTenantScopedEntities || (HasTenant && e.TenantId == CurrentTenantId)
                var contextExpr = Expression.Constant(this);
                var filterTenantScopedProp = Expression.Property(contextExpr, nameof(FilterTenantScopedEntities));
                var bypassActive = Expression.Not(filterTenantScopedProp);

                var hasTenantProp = Expression.Property(contextExpr, nameof(HasTenant));
                var tenantIdProp = Expression.Property(parameter, nameof(ITenantScoped.TenantId));
                var currentTenantIdProp = Expression.Property(contextExpr, nameof(CurrentTenantId));
                var tenantEquals = Expression.Equal(tenantIdProp, currentTenantIdProp);

                var tenantMatches = Expression.AndAlso(hasTenantProp, tenantEquals);
                var tenantFilter = Expression.OrElse(bypassActive, tenantMatches);

                filterExpression = filterExpression == null
                    ? tenantFilter
                    : Expression.AndAlso(filterExpression, tenantFilter);
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

        // Process Tenant Scoping and Cross-Tenant Integrity
        if (FilterTenantScopedEntities)
        {
            foreach (var entry in ChangeTracker.Entries<ITenantScoped>())
            {
                var tenantIdProperty = entry.Property(nameof(ITenantScoped.TenantId));

                switch (entry.State)
                {
                    case EntityState.Added:
                        if (!HasTenant)
                        {
                            var providedTenantId = (Guid)tenantIdProperty.CurrentValue!;
                            if (providedTenantId == Guid.Empty)
                            {
                                throw new CrossTenantViolationException(
                                    $"Cannot create tenant-scoped entity '{entry.Entity.GetType().Name}' without a valid tenant context.",
                                    "TENANT_CONTEXT_REQUIRED");
                            }
                        }
                        else
                        {
                            var currentTenantValue = (Guid)tenantIdProperty.CurrentValue!;
                            if (currentTenantValue == Guid.Empty)
                            {
                                tenantIdProperty.CurrentValue = CurrentTenantId;
                            }
                            else if (currentTenantValue != CurrentTenantId)
                            {
                                throw new CrossTenantViolationException(
                                    $"Cannot create entity for tenant '{currentTenantValue}' while authenticated in tenant '{CurrentTenantId}'.",
                                    "CROSS_TENANT_INSERT_DENIED");
                            }
                        }
                        break;

                    case EntityState.Modified:
                        if (tenantIdProperty.IsModified)
                        {
                            throw new CrossTenantViolationException(
                                $"Tenant ID is immutable and cannot be modified on entity '{entry.Entity.GetType().Name}'.",
                                "TENANT_IMMUTABLE");
                        }

                        if (HasTenant)
                        {
                            var entityTenantId = (Guid)tenantIdProperty.OriginalValue!;
                            if (entityTenantId != CurrentTenantId)
                            {
                                throw new CrossTenantViolationException(
                                    $"Cannot modify entity '{entry.Entity.GetType().Name}' belonging to tenant '{entityTenantId}'.",
                                    "CROSS_TENANT_UPDATE_DENIED");
                            }
                        }
                        break;

                    case EntityState.Deleted:
                        if (HasTenant)
                        {
                            var entityTenantId = (Guid)tenantIdProperty.OriginalValue!;
                            if (entityTenantId != CurrentTenantId)
                            {
                                throw new CrossTenantViolationException(
                                    $"Cannot delete entity '{entry.Entity.GetType().Name}' belonging to tenant '{entityTenantId}'.",
                                    "CROSS_TENANT_DELETE_DENIED");
                            }
                        }
                        break;
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
