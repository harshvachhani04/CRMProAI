using System.Net;
using System.Text.Json;
using CRMProAI.Application.Exceptions;
using CRMProAI.Contracts.Common;
using CRMProAI.Domain.Exceptions;

namespace CRMProAI.Api.Middleware;

/// <summary>
/// Intercepts all unhandled exceptions, sanitizes sensitive data, and returns RFC 7807 ProblemDetails.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next, 
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() 
                            ?? context.TraceIdentifier;

        var (statusCode, problemDetails) = MapExceptionToProblemDetails(context, exception, correlationId);

        _logger.LogError(
            exception,
            "An error occurred while processing request {Path} [TraceId: {CorrelationId}]: {Message}",
            context.Request.Path,
            correlationId,
            exception.Message);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(problemDetails, options);
        await context.Response.WriteAsync(json);
    }

    public static (HttpStatusCode StatusCode, ApiProblemDetails Problem) MapExceptionToProblemDetails(
        HttpContext context, 
        Exception exception, 
        string traceId)
    {
        var path = context.Request.Path.Value ?? "/";

        return exception switch
        {
            ValidationException valEx => (
                HttpStatusCode.BadRequest,
                new ApiProblemDetails
                {
                    Type = "https://crmpro.ai/errors/validation-failed",
                    Title = "Validation Failed",
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = "One or more validation errors occurred.",
                    Instance = path,
                    Code = "VALIDATION_ERROR",
                    TraceId = traceId,
                    Errors = valEx.Errors
                }),

            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                new ApiProblemDetails
                {
                    Type = "https://crmpro.ai/errors/not-found",
                    Title = "Resource Not Found",
                    Status = (int)HttpStatusCode.NotFound,
                    Detail = notFoundEx.Message,
                    Instance = path,
                    Code = notFoundEx.Code,
                    TraceId = traceId
                }),

            ConflictException conflictEx => (
                HttpStatusCode.Conflict,
                new ApiProblemDetails
                {
                    Type = "https://crmpro.ai/errors/conflict",
                    Title = "Business Conflict",
                    Status = (int)HttpStatusCode.Conflict,
                    Detail = conflictEx.Message,
                    Instance = path,
                    Code = conflictEx.Code,
                    TraceId = traceId
                }),

            ForbiddenException forbiddenEx => (
                HttpStatusCode.Forbidden,
                new ApiProblemDetails
                {
                    Type = "https://crmpro.ai/errors/forbidden",
                    Title = "Access Forbidden",
                    Status = (int)HttpStatusCode.Forbidden,
                    Detail = forbiddenEx.Message,
                    Instance = path,
                    Code = forbiddenEx.Code,
                    TraceId = traceId
                }),

            DomainException domainEx => (
                HttpStatusCode.UnprocessableEntity,
                new ApiProblemDetails
                {
                    Type = "https://crmpro.ai/errors/domain-rule-violation",
                    Title = "Domain Rule Violation",
                    Status = (int)HttpStatusCode.UnprocessableEntity,
                    Detail = domainEx.Message,
                    Instance = path,
                    Code = domainEx.Code,
                    TraceId = traceId
                }),

            _ => (
                HttpStatusCode.InternalServerError,
                new ApiProblemDetails
                {
                    Type = "https://crmpro.ai/errors/internal-server-error",
                    Title = "Internal Server Error",
                    Status = (int)HttpStatusCode.InternalServerError,
                    Detail = "An unexpected error occurred. Please quote your trace ID when contacting support.",
                    Instance = path,
                    Code = "INTERNAL_SERVER_ERROR",
                    TraceId = traceId
                })
        };
    }
}
