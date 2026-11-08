using System.Text.RegularExpressions;
using CRMProAI.Domain.Common;
using CRMProAI.Domain.Exceptions;
using CRMProAI.Domain.Modules.Tenancy.Events;

namespace CRMProAI.Domain.Modules.Tenancy;

/// <summary>
/// Root Aggregate for multi-tenant company workspace isolation.
/// Governs organization identity, settings, status lifecycle, and workspace defaults.
/// </summary>
public class Tenant : AggregateRoot<Guid>, IAuditableEntity, ISoftDeletable
{
    private static readonly Regex SlugRegex = new(
        "^[a-z0-9][a-z0-9-]{0,61}[a-z0-9]$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? LegalName { get; private set; }
    public TenantStatus Status { get; private set; } = TenantStatus.Provisioning;
    public TenantSettings Settings { get; private set; } = TenantSettings.Default();

    // Auditable Entity
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }

    // Soft Delete
    public bool IsArchived { get; private set; }
    public DateTimeOffset? ArchivedAtUtc { get; private set; }
    public Guid? ArchivedByUserId { get; private set; }

    private Tenant() { }

    private Tenant(
        Guid id,
        string name,
        string slug,
        string? legalName,
        TenantStatus status,
        TenantSettings settings)
        : base(id)
    {
        ValidateName(name);
        ValidateSlug(slug);

        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        LegalName = string.IsNullOrWhiteSpace(legalName) ? null : legalName.Trim();
        Status = status;
        Settings = settings ?? TenantSettings.Default();

        AddDomainEvent(new TenantCreatedDomainEvent(Id, Slug, Name));
    }

    public static Tenant Create(
        Guid id,
        string name,
        string slug,
        string? legalName = null,
        TenantSettings? settings = null)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Tenant ID cannot be empty.", "INVALID_TENANT_ID");
        }

        return new Tenant(
            id,
            name,
            slug,
            legalName,
            TenantStatus.Active,
            settings ?? TenantSettings.Default());
    }

    public void UpdateProfile(string name, string? legalName, TenantSettings settings)
    {
        ValidateName(name);

        Name = name.Trim();
        LegalName = string.IsNullOrWhiteSpace(legalName) ? null : legalName.Trim();
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public void Activate()
    {
        if (Status == TenantStatus.Active)
        {
            return;
        }

        var oldStatus = Status;
        Status = TenantStatus.Active;
        AddDomainEvent(new TenantStatusChangedDomainEvent(Id, oldStatus, Status, "Tenant activated"));
    }

    public void Suspend(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("A reason must be provided to suspend a tenant.", "SUSPENSION_REASON_REQUIRED");
        }

        if (Status == TenantStatus.Suspended)
        {
            return;
        }

        var oldStatus = Status;
        Status = TenantStatus.Suspended;
        AddDomainEvent(new TenantStatusChangedDomainEvent(Id, oldStatus, Status, reason.Trim()));
    }

    public void Archive(Guid? actorUserId)
    {
        if (IsArchived)
        {
            return;
        }

        IsArchived = true;
        ArchivedAtUtc = DateTimeOffset.UtcNow;
        ArchivedByUserId = actorUserId;
        Status = TenantStatus.Suspended;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Tenant organization name cannot be empty.", "INVALID_TENANT_NAME");
        }

        if (name.Trim().Length > 200)
        {
            throw new DomainException("Tenant organization name must not exceed 200 characters.", "TENANT_NAME_TOO_LONG");
        }
    }

    private static void ValidateSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new DomainException("Tenant slug cannot be empty.", "INVALID_TENANT_SLUG");
        }

        var normalized = slug.Trim().ToLowerInvariant();

        if (normalized.Length < 2 || normalized.Length > 63 || !SlugRegex.IsMatch(normalized))
        {
            throw new DomainException(
                "Tenant slug must be 2 to 63 lowercase alphanumeric characters or hyphens, starting and ending with an alphanumeric character.",
                "INVALID_TENANT_SLUG_FORMAT");
        }
    }
}
