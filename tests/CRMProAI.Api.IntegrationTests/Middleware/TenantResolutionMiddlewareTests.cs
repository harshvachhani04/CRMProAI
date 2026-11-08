using System.Security.Claims;
using CRMProAI.Api.Middleware;
using CRMProAI.Application.Common;
using CRMProAI.Domain.Exceptions;
using CRMProAI.Domain.Modules.Tenancy;
using CRMProAI.Infrastructure.Persistence;
using CRMProAI.Infrastructure.Persistence.Seed;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CRMProAI.Api.IntegrationTests.Middleware;

public class TenantResolutionMiddlewareTests
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=crmpro_dev;Username=postgres;Password=Harsh@123";

    private ApplicationDbContext CreateDbContext()
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        builder.UseNpgsql(ConnectionString);
        builder.UseSnakeCaseNamingConvention();

        return new ApplicationDbContext(builder.Options);
    }

    [Theory]
    [InlineData("northstar.crmpro.ai", "northstar")]
    [InlineData("acme-corp.crmpro.ai:5001", "acme-corp")]
    [InlineData("api.crmpro.ai", null)]
    [InlineData("www.crmpro.ai", null)]
    [InlineData("localhost", null)]
    [InlineData("127.0.0.1", null)]
    public void ExtractSubdomain_ShouldReturnExpectedSlug(string host, string? expectedSubdomain)
    {
        var result = TenantResolutionMiddleware.ExtractSubdomain(host);
        result.Should().Be(expectedSubdomain);
    }

    [Fact]
    public async Task InvokeAsync_WhenUnauthenticatedAndNoHeaders_ShouldProceedWithNoTenant()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var tenantContext = new TenantContext();
        var context = new DefaultHttpContext();

        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new TenantResolutionMiddleware(next, NullLogger<TenantResolutionMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context, dbContext, tenantContext);

        // Assert
        nextCalled.Should().BeTrue();
        tenantContext.HasTenant.Should().BeFalse();
        context.Items.ContainsKey("TenantId").Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_WithTenantSlugHeader_ShouldResolveDemoTenant()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        await TenantSeed.SeedDemoTenantAsync(dbContext);

        var tenantContext = new TenantContext();
        var context = new DefaultHttpContext();
        context.Request.Headers[TenantResolutionMiddleware.HeaderTenantSlug] = TenantSeed.DemoTenantSlug;

        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new TenantResolutionMiddleware(next, NullLogger<TenantResolutionMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context, dbContext, tenantContext);

        // Assert
        nextCalled.Should().BeTrue();
        tenantContext.HasTenant.Should().BeTrue();
        tenantContext.TenantId.Should().Be(TenantSeed.DemoTenantId);
        tenantContext.TenantSlug.Should().Be(TenantSeed.DemoTenantSlug);
        context.Items["TenantId"].Should().Be(TenantSeed.DemoTenantId);
    }

    [Fact]
    public async Task InvokeAsync_WithTenantIdHeader_ShouldResolveDemoTenant()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        await TenantSeed.SeedDemoTenantAsync(dbContext);

        var tenantContext = new TenantContext();
        var context = new DefaultHttpContext();
        context.Request.Headers[TenantResolutionMiddleware.HeaderTenantId] = TenantSeed.DemoTenantId.ToString();

        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new TenantResolutionMiddleware(next, NullLogger<TenantResolutionMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context, dbContext, tenantContext);

        // Assert
        nextCalled.Should().BeTrue();
        tenantContext.HasTenant.Should().BeTrue();
        tenantContext.TenantId.Should().Be(TenantSeed.DemoTenantId);
    }

    [Fact]
    public async Task InvokeAsync_WithAuthenticatedTenantClaim_ShouldResolveTenant()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        await TenantSeed.SeedDemoTenantAsync(dbContext);

        var tenantContext = new TenantContext();
        var context = new DefaultHttpContext();

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(TenantResolutionMiddleware.ClaimTenantId, TenantSeed.DemoTenantId.ToString())
        }, "Bearer");
        context.User = new ClaimsPrincipal(identity);

        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new TenantResolutionMiddleware(next, NullLogger<TenantResolutionMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context, dbContext, tenantContext);

        // Assert
        nextCalled.Should().BeTrue();
        tenantContext.HasTenant.Should().BeTrue();
        tenantContext.TenantId.Should().Be(TenantSeed.DemoTenantId);
    }

    [Fact]
    public async Task InvokeAsync_WhenClientAttemptsUntrustedTenantIdOverride_ShouldThrowForbiddenException()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        await TenantSeed.SeedDemoTenantAsync(dbContext);

        var tenantContext = new TenantContext();
        var context = new DefaultHttpContext();

        // Authenticated user belongs to DemoTenant
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(TenantResolutionMiddleware.ClaimTenantId, TenantSeed.DemoTenantId.ToString())
        }, "Bearer");
        context.User = new ClaimsPrincipal(identity);

        // Attacker passes a different tenant ID in X-Tenant-ID header
        var hostileTenantId = Guid.NewGuid();
        context.Request.Headers[TenantResolutionMiddleware.HeaderTenantId] = hostileTenantId.ToString();

        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new TenantResolutionMiddleware(next, NullLogger<TenantResolutionMiddleware>.Instance);

        // Act & Assert
        var act = async () => await middleware.InvokeAsync(context, dbContext, tenantContext);
        var ex = await act.Should().ThrowAsync<ForbiddenException>();
        ex.Which.Code.Should().Be("UNTRUSTED_TENANT_OVERRIDE");
    }

    [Fact]
    public async Task InvokeAsync_WhenTenantSuspended_ShouldThrowForbiddenException()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        var suspendedTenantId = Guid.NewGuid();
        var suspendedTenant = Tenant.Create(suspendedTenantId, "Suspended Org", $"suspended-org-{Guid.NewGuid():N}");
        suspendedTenant.Suspend("Account delinquent");
        dbContext.Tenants.Add(suspendedTenant);
        await dbContext.SaveChangesAsync();

        var tenantContext = new TenantContext();
        var context = new DefaultHttpContext();
        context.Request.Headers[TenantResolutionMiddleware.HeaderTenantId] = suspendedTenantId.ToString();

        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new TenantResolutionMiddleware(next, NullLogger<TenantResolutionMiddleware>.Instance);

        // Act & Assert
        var act = async () => await middleware.InvokeAsync(context, dbContext, tenantContext);
        var ex = await act.Should().ThrowAsync<ForbiddenException>();
        ex.Which.Code.Should().Be("TENANT_SUSPENDED");
    }
}
