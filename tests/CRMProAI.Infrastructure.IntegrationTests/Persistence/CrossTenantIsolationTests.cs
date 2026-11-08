using CRMProAI.Application.Common;
using CRMProAI.Domain.Common;
using CRMProAI.Domain.Entities;
using CRMProAI.Domain.Exceptions;
using CRMProAI.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CRMProAI.Infrastructure.IntegrationTests.Persistence;

public class CrossTenantIsolationTests
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
    public async Task Query_WhenAuthenticatedAsTenantA_ShouldOnlyReturnTenantARecords()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var probeA = new TenantProbe(Guid.NewGuid(), tenantA, "Probe A", "Created by Tenant A");
        var probeB = new TenantProbe(Guid.NewGuid(), tenantB, "Probe B", "Created by Tenant B");

        // Seed via platform bypass
        await using (var seedContext = CreateDbContext())
        using (seedContext.CreateTenantFilterBypassScope())
        {
            seedContext.TenantProbes.AddRange(probeA, probeB);
            await seedContext.SaveChangesAsync();
        }

        // Act - Query under Tenant A context
        var contextA = new TenantContext(tenantA);
        await using var queryContextA = CreateDbContext(contextA);

        var tenantAProbes = await queryContextA.TenantProbes.ToListAsync();

        // Assert
        tenantAProbes.Should().ContainSingle(p => p.Id == probeA.Id);
        tenantAProbes.Should().NotContain(p => p.Id == probeB.Id);
        tenantAProbes.All(p => p.TenantId == tenantA).Should().BeTrue();
    }

    [Fact]
    public async Task Query_WhenAuthenticatedAsTenantB_ShouldOnlyReturnTenantBRecords()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var probeA = new TenantProbe(Guid.NewGuid(), tenantA, "Probe A", "Created by Tenant A");
        var probeB = new TenantProbe(Guid.NewGuid(), tenantB, "Probe B", "Created by Tenant B");

        // Seed via platform bypass
        await using (var seedContext = CreateDbContext())
        using (seedContext.CreateTenantFilterBypassScope())
        {
            seedContext.TenantProbes.AddRange(probeA, probeB);
            await seedContext.SaveChangesAsync();
        }

        // Act - Query under Tenant B context
        var contextB = new TenantContext(tenantB);
        await using var queryContextB = CreateDbContext(contextB);

        var tenantBProbes = await queryContextB.TenantProbes.ToListAsync();

        // Assert
        tenantBProbes.Should().ContainSingle(p => p.Id == probeB.Id);
        tenantBProbes.Should().NotContain(p => p.Id == probeA.Id);
        tenantBProbes.All(p => p.TenantId == tenantB).Should().BeTrue();
    }

    [Fact]
    public async Task FindById_WhenAttemptingToReadOtherTenantsEntity_ShouldReturnNull()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var probeB = new TenantProbe(Guid.NewGuid(), tenantB, "Probe B", "Exclusive to B");

        await using (var seedContext = CreateDbContext())
        using (seedContext.CreateTenantFilterBypassScope())
        {
            seedContext.TenantProbes.Add(probeB);
            await seedContext.SaveChangesAsync();
        }

        // Act - Tenant A attempts to load Tenant B's record by direct primary key
        var contextA = new TenantContext(tenantA);
        await using var queryContextA = CreateDbContext(contextA);

        var queriedRecord = await queryContextA.TenantProbes.FirstOrDefaultAsync(p => p.Id == probeB.Id);

        // Assert
        queriedRecord.Should().BeNull("Global query filter must deny direct ID lookup across tenant boundaries");
    }

    [Fact]
    public async Task Query_WhenNoTenantContextProvided_ShouldReturnEmpty_WithoutLeakingData()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var probeA = new TenantProbe(Guid.NewGuid(), tenantA, "Probe A", "Confidential A");

        await using (var seedContext = CreateDbContext())
        using (seedContext.CreateTenantFilterBypassScope())
        {
            seedContext.TenantProbes.Add(probeA);
            await seedContext.SaveChangesAsync();
        }

        // Act - DbContext without tenant context (e.g. unauthenticated request)
        await using var unauthContext = CreateDbContext(new TenantContext());
        var results = await unauthContext.TenantProbes.ToListAsync();

        // Assert
        results.Should().NotContain(p => p.Id == probeA.Id, "Unauthenticated queries must fail closed and return zero tenant records");
    }

    [Fact]
    public async Task CreateTenantFilterBypassScope_ShouldAllowCrossTenantPlatformQueries()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var probeA = new TenantProbe(Guid.NewGuid(), tenantA, "Probe A", "Tenant A item");
        var probeB = new TenantProbe(Guid.NewGuid(), tenantB, "Probe B", "Tenant B item");

        await using (var seedContext = CreateDbContext())
        using (seedContext.CreateTenantFilterBypassScope())
        {
            seedContext.TenantProbes.AddRange(probeA, probeB);
            await seedContext.SaveChangesAsync();
        }

        // Act & Assert - With bypass scope
        var contextA = new TenantContext(tenantA);
        await using var dbContext = CreateDbContext(contextA);

        // Under normal scoping: only Tenant A is visible
        var normalResults = await dbContext.TenantProbes.ToListAsync();
        normalResults.Should().ContainSingle(p => p.Id == probeA.Id);
        normalResults.Should().NotContain(p => p.Id == probeB.Id);

        // Inside bypass scope: both Tenant A and Tenant B are visible for platform operations
        using (dbContext.CreateTenantFilterBypassScope())
        {
            var bypassResults = await dbContext.TenantProbes.ToListAsync();
            bypassResults.Should().Contain(p => p.Id == probeA.Id);
            bypassResults.Should().Contain(p => p.Id == probeB.Id);
        }

        // Outside bypass scope: filter automatically re-engages
        var restoredResults = await dbContext.TenantProbes.ToListAsync();
        restoredResults.Should().ContainSingle(p => p.Id == probeA.Id);
        restoredResults.Should().NotContain(p => p.Id == probeB.Id);
    }

    [Fact]
    public async Task CreateTenantFilterBypassScope_ShouldPreserveSoftDeleteFilter()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var activeProbe = new TenantProbe(Guid.NewGuid(), tenantA, "Active", "Visible");
        var archivedProbe = new TenantProbe(Guid.NewGuid(), tenantB, "Archived", "Deleted");

        await using (var seedContext = CreateDbContext())
        using (seedContext.CreateTenantFilterBypassScope())
        {
            seedContext.TenantProbes.AddRange(activeProbe, archivedProbe);
            await seedContext.SaveChangesAsync();

            // Soft-delete the second probe
            seedContext.TenantProbes.Remove(archivedProbe);
            await seedContext.SaveChangesAsync();
        }

        // Act - Query with tenant bypass
        await using var platformContext = CreateDbContext();
        using (platformContext.CreateTenantFilterBypassScope())
        {
            var results = await platformContext.TenantProbes.ToListAsync();

            // Assert - soft-delete filter must still exclude archivedProbe
            results.Should().Contain(p => p.Id == activeProbe.Id);
            results.Should().NotContain(p => p.Id == archivedProbe.Id, "Tenant bypass must preserve soft delete filter");

            // But IgnoreQueryFilters() clears all filters including soft delete
            var rawResults = await platformContext.TenantProbes.IgnoreQueryFilters().ToListAsync();
            rawResults.Should().Contain(p => p.Id == archivedProbe.Id);
        }
    }

    [Fact]
    public async Task SaveChanges_WhenAddingEntityWithoutExplicitTenantId_ShouldAutoStampCurrentTenantId()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var contextA = new TenantContext(tenantA);
        await using var dbContext = CreateDbContext(contextA);

        // Create probe with Guid.Empty
        var probe = new TenantProbe(Guid.NewGuid(), Guid.Empty, "Auto Stamp Test", "Auto stamp description");
        dbContext.TenantProbes.Add(probe);
        await dbContext.SaveChangesAsync();

        // Assert
        probe.TenantId.Should().Be(tenantA, "EF Core SaveChangesAsync must automatically stamp CurrentTenantId when empty");
    }

    [Fact]
    public async Task SaveChanges_WhenAddingEntityForDifferentTenant_ShouldThrowCrossTenantViolation()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var hostileTenantB = Guid.NewGuid();

        var contextA = new TenantContext(tenantA);
        await using var dbContext = CreateDbContext(contextA);

        // Attacker in Tenant A tries to insert data into Tenant B
        var hostileProbe = new TenantProbe(Guid.NewGuid(), hostileTenantB, "Hostile Insert", "Payload");
        dbContext.TenantProbes.Add(hostileProbe);

        // Act & Assert
        var act = async () => await dbContext.SaveChangesAsync();
        var ex = await act.Should().ThrowAsync<CrossTenantViolationException>();
        ex.Which.Code.Should().Be("CROSS_TENANT_INSERT_DENIED");
    }

    [Fact]
    public async Task SaveChanges_WhenModifyingEntityBelongingToAnotherTenant_ShouldThrowCrossTenantViolation()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var probeB = new TenantProbe(Guid.NewGuid(), tenantB, "Original B", "Details B");

        await using (var seedContext = CreateDbContext())
        using (seedContext.CreateTenantFilterBypassScope())
        {
            seedContext.TenantProbes.Add(probeB);
            await seedContext.SaveChangesAsync();
        }

        // Tenant A tries to attach and modify Tenant B's entity
        var contextA = new TenantContext(tenantA);
        await using var maliciousContext = CreateDbContext(contextA);

        maliciousContext.TenantProbes.Attach(probeB);
        probeB.Update("Tampered Title", "Tampered Details");

        // Act & Assert
        var act = async () => await maliciousContext.SaveChangesAsync();
        var ex = await act.Should().ThrowAsync<CrossTenantViolationException>();
        ex.Which.Code.Should().Be("CROSS_TENANT_UPDATE_DENIED");
    }

    [Fact]
    public async Task SaveChanges_WhenAttemptingToModifyTenantId_ShouldThrowCrossTenantViolation()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var contextA = new TenantContext(tenantA);
        await using var dbContext = CreateDbContext(contextA);

        var probe = new TenantProbe(Guid.NewGuid(), tenantA, "Probe", "Details");
        dbContext.TenantProbes.Add(probe);
        await dbContext.SaveChangesAsync();

        // Attempt to move probe to another tenant by changing TenantId property
        var hostileTenantC = Guid.NewGuid();
        dbContext.Entry(probe).Property(nameof(ITenantScoped.TenantId)).CurrentValue = hostileTenantC;

        // Act & Assert
        var act = async () => await dbContext.SaveChangesAsync();
        var ex = await act.Should().ThrowAsync<CrossTenantViolationException>();
        ex.Which.Code.Should().Be("TENANT_IMMUTABLE");
    }
}
