namespace CRMProAI.Application.Common;

/// <summary>
/// Provides access to the authenticated user ID and claims for auditing and authorization.
/// </summary>
public interface ICurrentUserContext
{
    Guid? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}
