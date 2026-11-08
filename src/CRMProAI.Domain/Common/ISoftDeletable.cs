namespace CRMProAI.Domain.Common;

/// <summary>
/// Supports soft-delete/archival of business records to preserve historical relationships.
/// </summary>
public interface ISoftDeletable
{
    bool IsArchived { get; }
    DateTimeOffset? ArchivedAtUtc { get; }
    Guid? ArchivedByUserId { get; }
}
