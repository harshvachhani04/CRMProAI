namespace CRMProAI.Domain.Common;

/// <summary>
/// Represents a domain event raised when significant business state changes occur.
/// </summary>
public interface IDomainEvent
{
    Guid EventId => Guid.NewGuid();
    DateTimeOffset OccurredOnUtc => DateTimeOffset.UtcNow;
}
