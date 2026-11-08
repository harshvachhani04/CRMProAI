namespace CRMProAI.Domain.Common;

/// <summary>
/// Enforces tenant ownership on all multi-tenant aggregates and entities.
/// A tenant-scoped entity must never have a null TenantId.
/// </summary>
public interface ITenantScoped
{
    Guid TenantId { get; }
}
