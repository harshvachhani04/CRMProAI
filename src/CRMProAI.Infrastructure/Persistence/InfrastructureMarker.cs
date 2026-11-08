using CRMProAI.Application.Common;
using CRMProAI.Domain.Common;

namespace CRMProAI.Infrastructure.Persistence;

/// <summary>
/// Assembly marker for CRMProAI.Infrastructure.
/// </summary>
public static class InfrastructureMarker
{
    public static readonly Type DomainType = typeof(ITenantScoped);
    public static readonly Type ApplicationType = typeof(ITenantContext);
}
