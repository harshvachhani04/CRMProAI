using CRMProAI.Domain.Entities;
using CRMProAI.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CRMProAI.Infrastructure.IntegrationTests.Persistence;

public class ApplicationDbContextTests
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=crmpro_dev;Username=postgres;Password=Harsh@123";

    private ApplicationDbContext CreateDbContext()
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        builder.UseNpgsql(ConnectionString);
        builder.UseSnakeCaseNamingConvention();

        return new ApplicationDbContext(builder.Options);
    }

    [Fact]
    public async Task CanConnectToDatabase_AndQueryMigrations()
    {
        await using var context = CreateDbContext();

        var canConnect = await context.Database.CanConnectAsync();
        canConnect.Should().BeTrue();

        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
        appliedMigrations.Should().Contain(m => m.Contains("InitialBaseline"));
    }

    [Fact]
    public async Task AddingEntity_ShouldAutomaticallyStampCreatedAtUtc()
    {
        await using var context = CreateDbContext();

        var probe = new BaselineProbe(Guid.NewGuid(), "Audit Test", "Testing automatic timestamp stamping");
        context.BaselineProbes.Add(probe);
        await context.SaveChangesAsync();

        probe.CreatedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        probe.UpdatedAtUtc.Should().BeNull();
        probe.IsArchived.Should().BeFalse();
    }

    [Fact]
    public async Task ModifyingEntity_ShouldAutomaticallyStampUpdatedAtUtc()
    {
        await using var context = CreateDbContext();

        var probe = new BaselineProbe(Guid.NewGuid(), "Update Test", "Original description");
        context.BaselineProbes.Add(probe);
        await context.SaveChangesAsync();

        var initialCreated = probe.CreatedAtUtc;

        // Act - Modify entity
        probe.Update("Updated Name", "Modified description");
        await context.SaveChangesAsync();

        probe.CreatedAtUtc.Should().Be(initialCreated);
        probe.UpdatedAtUtc.Should().NotBeNull();
        probe.UpdatedAtUtc!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SoftDeletingEntity_ShouldSetIsArchived_AndExcludeFromDefaultQueries()
    {
        await using var context = CreateDbContext();

        var probeId = Guid.NewGuid();
        var probe = new BaselineProbe(probeId, "Soft Delete Test", "Testing soft delete query filter");
        context.BaselineProbes.Add(probe);
        await context.SaveChangesAsync();

        // Act - Delete entity
        context.BaselineProbes.Remove(probe);
        await context.SaveChangesAsync();

        // Assert in fresh DbContext to verify query filter persistence
        await using var verifyContext = CreateDbContext();

        var activeItem = await verifyContext.BaselineProbes.FirstOrDefaultAsync(p => p.Id == probeId);
        activeItem.Should().BeNull("Global query filter must exclude soft-deleted entities");

        var rawItem = await verifyContext.BaselineProbes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == probeId);

        rawItem.Should().NotBeNull();
        rawItem!.IsArchived.Should().BeTrue();
        rawItem.ArchivedAtUtc.Should().NotBeNull();
        rawItem.ArchivedAtUtc!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }
}
