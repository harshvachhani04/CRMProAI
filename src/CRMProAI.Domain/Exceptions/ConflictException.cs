namespace CRMProAI.Domain.Exceptions;

/// <summary>
/// Thrown when an optimistic concurrency conflict or unique business constraint is violated.
/// </summary>
public class ConflictException : DomainException
{
    public ConflictException(string message, string code = "CONFLICT")
        : base(message, code)
    {
    }
}
