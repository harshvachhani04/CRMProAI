using CRMProAI.Api.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CRMProAI.Api.IntegrationTests.Middleware;

public class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenHeaderPresent_ShouldPreserveCorrelationId()
    {
        // Arrange
        const string expectedCorrelationId = "test-corr-id-12345";
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.CorrelationIdHeaderName] = expectedCorrelationId;

        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new CorrelationIdMiddleware(next, NullLogger<CorrelationIdMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Items["CorrelationId"].Should().Be(expectedCorrelationId);
    }

    [Fact]
    public async Task InvokeAsync_WhenHeaderMissing_ShouldGenerateNewCorrelationId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new CorrelationIdMiddleware(next, NullLogger<CorrelationIdMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        var correlationId = context.Items["CorrelationId"] as string;
        correlationId.Should().NotBeNullOrWhiteSpace();
        Guid.TryParse(correlationId, out _).Should().BeTrue();
    }
}
