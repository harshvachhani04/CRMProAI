using CRMProAI.Api.Controllers.V1;
using CRMProAI.Application.Common.Validation;
using CRMProAI.Contracts.Common;
using CRMProAI.Domain.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using AppValidationException = CRMProAI.Application.Exceptions.ValidationException;

namespace CRMProAI.Api.IntegrationTests.Controllers;

public class HealthControllerTests
{
    private readonly HealthController _controller;

    public HealthControllerTests()
    {
        var validator = new TestValidationRequestValidator();
        _controller = new HealthController(validator)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        _controller.HttpContext.Items["CorrelationId"] = "health-test-corr-id";
    }

    [Fact]
    public void GetHealth_ReturnsOkWithApiResponse()
    {
        // Act
        var result = _controller.GetHealth();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);

        var response = okResult.Value as ApiResponse<object>;
        response.Should().NotBeNull();
        response!.Message.Should().Be("CRMPro AI API operational");
    }

    [Fact]
    public void GetTenantContext_WhenTenantInContext_ReturnsOkWithTenantDetails()
    {
        // Arrange
        var testTenantId = Guid.NewGuid();
        _controller.HttpContext.Items["TenantId"] = testTenantId;
        _controller.HttpContext.Items["TenantSlug"] = "northstar";

        // Act
        var result = _controller.GetTenantContext();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);

        var response = okResult.Value as ApiResponse<object>;
        response.Should().NotBeNull();
        response!.Message.Should().Be("Tenant resolved successfully");
    }

    [Fact]
    public async Task TestValidation_WhenInvalidPayload_ThrowsValidationException()
    {
        // Arrange
        var invalidRequest = new TestValidationRequest(
            Name: "", // empty -> error
            Email: "invalid-email", // invalid -> error
            EstimatedValue: -10m // <= 0 -> error
        );

        // Act & Assert
        var act = async () => await _controller.TestValidation(invalidRequest, CancellationToken.None);
        var ex = await act.Should().ThrowAsync<AppValidationException>();
        ex.Which.Errors.Should().ContainKey("Name");
        ex.Which.Errors.Should().ContainKey("Email");
        ex.Which.Errors.Should().ContainKey("EstimatedValue");
    }

    [Fact]
    public async Task TestValidation_WhenValidPayload_ReturnsOk()
    {
        // Arrange
        var validRequest = new TestValidationRequest(
            Name: "John Doe",
            Email: "john.doe@example.com",
            EstimatedValue: 5000m
        );

        // Act
        var result = await _controller.TestValidation(validRequest, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<TestValidationRequest>;
        response.Should().NotBeNull();
        response!.Message.Should().Be("Validation passed successfully");
        response.Data.Should().Be(validRequest);
    }

    [Fact]
    public void TestConflict_ThrowsConflictException()
    {
        // Act & Assert
        var act = () => _controller.TestConflict();
        act.Should().Throw<ConflictException>()
            .WithMessage("*concurrency*");
    }

    [Fact]
    public void TestUnhandledError_ThrowsInvalidOperationException()
    {
        // Act & Assert
        var act = () => _controller.TestUnhandledError();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Sensitive*");
    }
}
