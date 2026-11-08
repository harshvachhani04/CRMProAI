namespace CRMProAI.Domain.Exceptions;

/// <summary>
/// Base exception for domain invariant violations.
/// </summary>
public class DomainException : Exception
{
    public string Code { get; }

    public DomainException(string message, string code = "DOMAIN_ERROR") 
        : base(message)
    {
        Code = code;
    }
}
