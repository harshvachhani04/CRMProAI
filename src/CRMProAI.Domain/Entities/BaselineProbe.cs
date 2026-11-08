using CRMProAI.Domain.Common;

namespace CRMProAI.Domain.Entities;

/// <summary>
/// Baseline probe entity used to verify EF Core conventions, snake_case naming,
/// UTC timestamp conversion, soft-delete filtering, and audit trail metadata.
/// </summary>
public class BaselineProbe : AggregateRoot<Guid>, IAuditableEntity, ISoftDeletable
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTimeOffset? ArchivedAtUtc { get; private set; }
    public Guid? ArchivedByUserId { get; private set; }

    private BaselineProbe() { }

    public BaselineProbe(Guid id, string name, string description) : base(id)
    {
        Name = name;
        Description = description;
    }

    public void Update(string name, string description)
    {
        Name = name;
        Description = description;
    }
}
