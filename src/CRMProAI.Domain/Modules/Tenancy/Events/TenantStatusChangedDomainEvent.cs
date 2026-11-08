using CRMProAI.Domain.Common;

namespace CRMProAI.Domain.Modules.Tenancy.Events;

public record TenantStatusChangedDomainEvent(
    Guid TenantId,
    TenantStatus OldStatus,
    TenantStatus NewStatus,
    string? Reason) : IDomainEvent
{
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
