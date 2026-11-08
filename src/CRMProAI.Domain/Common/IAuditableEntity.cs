namespace CRMProAI.Domain.Common;

/// <summary>
/// Provides standardized audit timestamps and actor IDs across all business entities.
/// </summary>
public interface IAuditableEntity
{
    DateTimeOffset CreatedAtUtc { get; }
    Guid? CreatedByUserId { get; }
    DateTimeOffset? UpdatedAtUtc { get; }
    Guid? UpdatedByUserId { get; }
}
