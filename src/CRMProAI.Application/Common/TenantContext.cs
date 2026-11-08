namespace CRMProAI.Application.Common;

public interface ITenantSetter
{
    void SetTenant(Guid tenantId, string? tenantSlug = null);
    void Clear();
}

/// <summary>
/// Scoped tenant context implementation holding the resolved tenant identity for the active scope/request.
/// </summary>
public class TenantContext : ITenantContext, ITenantSetter
{
    public Guid TenantId { get; private set; } = Guid.Empty;
    public string? TenantSlug { get; private set; }
    public bool HasTenant => TenantId != Guid.Empty;

    public TenantContext() { }

    public TenantContext(Guid tenantId, string? tenantSlug = null)
    {
        TenantId = tenantId;
        TenantSlug = tenantSlug;
    }

    public void SetTenant(Guid tenantId, string? tenantSlug = null)
    {
        TenantId = tenantId;
        TenantSlug = tenantSlug;
    }

    public void Clear()
    {
        TenantId = Guid.Empty;
        TenantSlug = null;
    }
}
