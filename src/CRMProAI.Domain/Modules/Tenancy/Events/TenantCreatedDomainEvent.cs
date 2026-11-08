using CRMProAI.Domain.Common;

namespace CRMProAI.Domain.Modules.Tenancy.Events;

public record TenantCreatedDomainEvent(
    Guid TenantId,
    string Slug,
    string Name) : IDomainEvent
{
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
