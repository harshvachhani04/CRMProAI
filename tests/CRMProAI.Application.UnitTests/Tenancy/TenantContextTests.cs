using CRMProAI.Application.Common;
using FluentAssertions;
using Xunit;

namespace CRMProAI.Application.UnitTests.Tenancy;

public class TenantContextTests
{
    [Fact]
    public void DefaultTenantContext_ShouldHaveEmptyTenantIdAndHasTenantFalse()
    {
        // Act
        var context = new TenantContext();

        // Assert
        context.TenantId.Should().Be(Guid.Empty);
        context.TenantSlug.Should().BeNull();
        context.HasTenant.Should().BeFalse();
    }

    [Fact]
    public void SetTenant_ShouldUpdateTenantIdAndHasTenantTrue()
    {
        // Arrange
        var context = new TenantContext();
        var tenantId = Guid.NewGuid();
        const string slug = "northstar";

        // Act
        context.SetTenant(tenantId, slug);

        // Assert
        context.TenantId.Should().Be(tenantId);
        context.TenantSlug.Should().Be("northstar");
        context.HasTenant.Should().BeTrue();
    }

    [Fact]
    public void Clear_ShouldResetTenantContext()
    {
        // Arrange
        var context = new TenantContext(Guid.NewGuid(), "some-slug");
        context.HasTenant.Should().BeTrue();

        // Act
        context.Clear();

        // Assert
        context.TenantId.Should().Be(Guid.Empty);
        context.TenantSlug.Should().BeNull();
        context.HasTenant.Should().BeFalse();
    }
}
