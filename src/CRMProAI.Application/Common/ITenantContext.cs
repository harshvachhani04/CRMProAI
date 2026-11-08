namespace CRMProAI.Application.Common;

/// <summary>
/// Provides access to the currently resolved tenant identity.
/// </summary>
public interface ITenantContext
{
    Guid TenantId { get; }
    string? TenantSlug { get; }
    bool HasTenant { get; }
}
