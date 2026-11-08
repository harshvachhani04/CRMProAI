using System.Net;
using System.Text.Json;
using CRMProAI.Api.Middleware;
using CRMProAI.Application.Exceptions;
using CRMProAI.Contracts.Common;
using CRMProAI.Domain.Exceptions;
using FluentAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CRMProAI.Api.IntegrationTests.Middleware;

public class GlobalExceptionMiddlewareTests
{
    private class DummyHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "CRMProAI.Api";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    [Fact]
    public void MapException_WhenValidationException_Returns400WithErrors()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/health/test-validation";
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required"),
            new("Score", "Score must be between 1 and 100")
        };
        var ex = new ValidationException(failures);

        // Act
        var (statusCode, problem) = GlobalExceptionMiddleware.MapExceptionToProblemDetails(context, ex, "trace-1");

        // Assert
        statusCode.Should().Be(HttpStatusCode.BadRequest);
        problem.Status.Should().Be(400);
        problem.Code.Should().Be("VALIDATION_ERROR");
        problem.Errors.Should().NotBeNull();
        problem.Errors.Should().ContainKey("Name");
        problem.Errors!["Name"].Should().Contain("Name is required");
        problem.Errors.Should().ContainKey("Score");
        problem.Errors!["Score"].Should().Contain("Score must be between 1 and 100");
    }

    [Fact]
    public void MapException_WhenConflictException_Returns409()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/health/test-conflict";
        var ex = new ConflictException("Concurrency token mismatch", "CONCURRENCY_CONFLICT");

        // Act
        var (statusCode, problem) = GlobalExceptionMiddleware.MapExceptionToProblemDetails(context, ex, "trace-2");

        // Assert
        statusCode.Should().Be(HttpStatusCode.Conflict);
        problem.Status.Should().Be(409);
        problem.Code.Should().Be("CONCURRENCY_CONFLICT");
        problem.Detail.Should().Be("Concurrency token mismatch");
    }

    [Fact]
    public void MapException_WhenNotFoundException_Returns404()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var ex = new NotFoundException("Account", Guid.NewGuid());

        // Act
        var (statusCode, problem) = GlobalExceptionMiddleware.MapExceptionToProblemDetails(context, ex, "trace-3");

        // Assert
        statusCode.Should().Be(HttpStatusCode.NotFound);
        problem.Status.Should().Be(404);
        problem.Code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public void MapException_WhenForbiddenException_Returns403()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var ex = new ForbiddenException("Tenant isolation breach detected");

        // Act
        var (statusCode, problem) = GlobalExceptionMiddleware.MapExceptionToProblemDetails(context, ex, "trace-4");

        // Assert
        statusCode.Should().Be(HttpStatusCode.Forbidden);
        problem.Status.Should().Be(403);
        problem.Code.Should().Be("FORBIDDEN");
    }

    [Fact]
    public void MapException_WhenUnhandledException_Returns500AndSanitizesMessage()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var ex = new InvalidOperationException("Sensitive internal database connection string: user=admin password=secret");

        // Act
        var (statusCode, problem) = GlobalExceptionMiddleware.MapExceptionToProblemDetails(context, ex, "trace-5");

        // Assert
        statusCode.Should().Be(HttpStatusCode.InternalServerError);
        problem.Status.Should().Be(500);
        problem.Code.Should().Be("INTERNAL_SERVER_ERROR");
        problem.Detail.Should().NotContain("secret");
        problem.Detail.Should().Contain("trace ID");
        problem.TraceId.Should().Be("trace-5");
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_WritesProblemDetailsJsonToResponse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;
        context.Items["CorrelationId"] = "trace-invoke-test";

        RequestDelegate next = _ => throw new ConflictException("Resource locked");
        var middleware = new GlobalExceptionMiddleware(
            next,
            NullLogger<GlobalExceptionMiddleware>.Instance,
            new DummyHostEnvironment());

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
        context.Response.ContentType.Should().Be("application/problem+json");

        responseStream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(responseStream);
        var body = await reader.ReadToEndAsync();
        body.Should().NotBeNullOrWhiteSpace();

        var problem = JsonSerializer.Deserialize<ApiProblemDetails>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        problem.Should().NotBeNull();
        problem!.Code.Should().Be("CONFLICT");
        problem.Detail.Should().Be("Resource locked");
        problem.TraceId.Should().Be("trace-invoke-test");
    }
}
