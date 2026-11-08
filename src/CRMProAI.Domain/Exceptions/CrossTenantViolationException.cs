namespace CRMProAI.Domain.Exceptions;

/// <summary>
/// Thrown when an operation attempts to read, insert, update, or delete data belonging to a different tenant,
/// or when a tenant context invariant is violated.
/// </summary>
public class CrossTenantViolationException : ForbiddenException
{
    public CrossTenantViolationException(
        string message = "Cross-tenant data access is strictly forbidden.",
        string code = "CROSS_TENANT_VIOLATION")
        : base(message, code)
    {
    }
}
