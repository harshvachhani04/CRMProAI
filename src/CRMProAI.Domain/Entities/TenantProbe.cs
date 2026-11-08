using CRMProAI.Domain.Common;

namespace CRMProAI.Domain.Entities;

/// <summary>
/// Tenant-scoped probe aggregate used to verify EF Core tenant query filters,
/// non-nullable TenantId constraints, cross-tenant isolation, and safe bypass policies.
/// </summary>
public class TenantProbe : AggregateRoot<Guid>, ITenantScoped, IAuditableEntity, ISoftDeletable
{
    public Guid TenantId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Details { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }

    public bool IsArchived { get; private set; }
    public DateTimeOffset? ArchivedAtUtc { get; private set; }
    public Guid? ArchivedByUserId { get; private set; }

    private TenantProbe() { }

    public TenantProbe(Guid id, Guid tenantId, string title, string details) : base(id)
    {
        TenantId = tenantId;
        Title = title;
        Details = details;
    }

    public void Update(string title, string details)
    {
        Title = title;
        Details = details;
    }
}
