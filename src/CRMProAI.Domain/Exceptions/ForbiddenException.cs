namespace CRMProAI.Domain.Exceptions;

/// <summary>
/// Thrown when the current user does not have permission for the requested action or tenant.
/// </summary>
public class ForbiddenException : DomainException
{
    public ForbiddenException(string message = "You do not have permission to perform this action.", string code = "FORBIDDEN")
        : base(message, code)
    {
    }
}
