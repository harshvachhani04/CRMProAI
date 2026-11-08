namespace CRMProAI.Contracts.Common;

/// <summary>
/// Standard response envelope for successful API requests.
/// </summary>
public record ApiResponse<T>
{
    public T Data { get; init; } = default!;
    public string Message { get; init; } = "Success";
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public ApiResponse() { }

    public ApiResponse(T data, string message = "Success", DateTimeOffset? timestamp = null)
    {
        Data = data;
        Message = message;
        Timestamp = timestamp ?? DateTimeOffset.UtcNow;
    }
}
