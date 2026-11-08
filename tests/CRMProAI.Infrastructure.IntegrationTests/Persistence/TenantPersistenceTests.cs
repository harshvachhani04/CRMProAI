using CRMProAI.Application.Common;
using CRMProAI.Domain.Modules.Tenancy;
using CRMProAI.Infrastructure.Persistence;
using CRMProAI.Infrastructure.Persistence.Seed;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CRMProAI.Infrastructure.IntegrationTests.Persistence;

public class TenantPersistenceTests
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=crmpro_dev;Username=postgres;Password=Harsh@123";

    private ApplicationDbContext CreateDbContext(ITenantContext? tenantContext = null)
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        builder.UseNpgsql(ConnectionString);
        builder.UseSnakeCaseNamingConvention();

        return new ApplicationDbContext(builder.Options, tenantContext);
    }

    [Fact]
    public async Task SeedDemoTenant_ShouldPersistNorthstarTenant()
    {
        await using var context = CreateDbContext();

        // Act - Seed demo tenant
        await TenantSeed.SeedDemoTenantAsync(context);

        // Assert
        var demoTenant = await context.Tenants
            .FirstOrDefaultAsync(t => t.Id == TenantSeed.DemoTenantId);

        demoTenant.Should().NotBeNull();
        demoTenant!.Name.Should().Be(TenantSeed.DemoTenantName);
        demoTenant.Slug.Should().Be(TenantSeed.DemoTenantSlug);
        demoTenant.Status.Should().Be(TenantStatus.Active);
        demoTenant.Settings.DefaultCurrency.Should().Be("USD");
        demoTenant.Settings.DefaultTimezone.Should().Be("America/New_York");
    }

    [Fact]
    public async Task QueryingTenantBySlug_ShouldReturnMatchingTenant()
    {
        await using var context = CreateDbContext();
        await TenantSeed.SeedDemoTenantAsync(context);

        // Act
        var tenant = await context.Tenants
            .FirstOrDefaultAsync(t => t.Slug == TenantSeed.DemoTenantSlug);

        // Assert
        tenant.Should().NotBeNull();
        tenant!.Id.Should().Be(TenantSeed.DemoTenantId);
        tenant.Name.Should().Be("Northstar Digital");
    }

    [Fact]
    public async Task InsertingDuplicateSlug_ShouldViolateUniqueIndexConstraint()
    {
        await using var context = CreateDbContext();
        await TenantSeed.SeedDemoTenantAsync(context);

        // Act - Try creating another tenant with the same slug "northstar"
        var duplicateTenant = Tenant.Create(
            Guid.NewGuid(),
            "Another Northstar",
            TenantSeed.DemoTenantSlug);

        context.Tenants.Add(duplicateTenant);

        var act = async () => await context.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task TenantCanBeLoaded_AndConfiguredIntoTrustedContext()
    {
        await using var context = CreateDbContext();
        await TenantSeed.SeedDemoTenantAsync(context);

        // Act - Load tenant from DB
        var tenant = await context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Slug == TenantSeed.DemoTenantSlug);

        tenant.Should().NotBeNull();

        // Configure trusted TenantContext
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(tenant!.Id, tenant.Slug);

        // Assert
        tenantContext.HasTenant.Should().BeTrue();
        tenantContext.TenantId.Should().Be(TenantSeed.DemoTenantId);
        tenantContext.TenantSlug.Should().Be("northstar");
    }
}
