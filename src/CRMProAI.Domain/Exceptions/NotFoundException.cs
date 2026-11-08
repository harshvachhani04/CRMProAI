namespace CRMProAI.Domain.Exceptions;

/// <summary>
/// Thrown when an entity or resource is not found or is inaccessible.
/// </summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with identifier '{key}' was not found.", "NOT_FOUND")
    {
    }

    public NotFoundException(string message)
        : base(message, "NOT_FOUND")
    {
    }
}
