namespace CRMProAI.Contracts.Common;

/// <summary>
/// Standard pagination wrapper for API list endpoints.
/// </summary>
public record PagedList<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
