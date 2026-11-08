using Asp.Versioning;
using CRMProAI.Application.Common.Validation;
using AppValidationException = CRMProAI.Application.Exceptions.ValidationException;
using CRMProAI.Contracts.Common;
using CRMProAI.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CRMProAI.Api.Controllers.V1;

[ApiVersion("1.0")]
public class HealthController : BaseApiController
{
    private readonly IValidator<TestValidationRequest> _validator;

    public HealthController(IValidator<TestValidationRequest> validator)
    {
        _validator = validator;
    }

    /// <summary>
    /// Health probe verifying API readiness and correlation tracking.
    /// </summary>
    [HttpGet]
    public ActionResult<ApiResponse<object>> GetHealth()
    {
        return OkResponse<object>(new
        {
            status = "Healthy",
            version = "v1.0",
            correlationId = CorrelationId,
            timestampUtc = DateTimeOffset.UtcNow
        }, "CRMPro AI API operational");
    }

    /// <summary>
    /// Test endpoint demonstrating validation failure handling (400 Bad Request ProblemDetails).
    /// </summary>
    [HttpPost("test-validation")]
    public async Task<ActionResult<ApiResponse<TestValidationRequest>>> TestValidation(
        [FromBody] TestValidationRequest request,
        CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            throw new AppValidationException(result.Errors);
        }

        return OkResponse(request, "Validation passed successfully");
    }

    /// <summary>
    /// Test endpoint demonstrating conflict failure handling (409 Conflict ProblemDetails).
    /// </summary>
    [HttpGet("test-conflict")]
    public ActionResult TestConflict()
    {
        throw new ConflictException("Another user has already modified this record. Concurrency token mismatch.");
    }

    /// <summary>
    /// Test endpoint demonstrating unhandled exception sanitization (500 Internal Server Error ProblemDetails).
    /// </summary>
    [HttpGet("test-error")]
    public ActionResult TestUnhandledError()
    {
        throw new InvalidOperationException("Sensitive internal database connection failed: secret_connection_string_xyz");
    }
}
