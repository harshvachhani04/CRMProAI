namespace CRMProAI.Api.Middleware;

/// <summary>
/// Ensures every incoming request has a unique correlation ID for end-to-end tracing.
/// Propagates the ID in the response headers and logging context.
/// </summary>
public class CorrelationIdMiddleware
{
    public const string CorrelationIdHeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId;

        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var values) &&
            !string.IsNullOrWhiteSpace(values.FirstOrDefault()))
        {
            correlationId = values.First()!;
        }
        else
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        context.Items["CorrelationId"] = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            await _next(context);
        }
    }
}
