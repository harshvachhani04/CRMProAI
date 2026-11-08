using CRMProAI.Domain.Exceptions;
using CRMProAI.Domain.Modules.Tenancy;
using CRMProAI.Domain.Modules.Tenancy.Events;
using FluentAssertions;
using Xunit;

namespace CRMProAI.Domain.UnitTests.Tenancy;

public class TenantTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateTenantAndRaiseDomainEvent()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var name = "Northstar Digital";
        var slug = "northstar-digital";
        var settings = new TenantSettings("UTC", "USD", "en-US", "admin@northstar.io");

        // Act
        var tenant = Tenant.Create(tenantId, name, slug, "Northstar Legal Inc.", settings);

        // Assert
        tenant.Id.Should().Be(tenantId);
        tenant.Name.Should().Be("Northstar Digital");
        tenant.Slug.Should().Be("northstar-digital");
        tenant.LegalName.Should().Be("Northstar Legal Inc.");
        tenant.Status.Should().Be(TenantStatus.Active);
        tenant.Settings.DefaultCurrency.Should().Be("USD");
        tenant.Settings.DefaultTimezone.Should().Be("UTC");
        tenant.Settings.SupportEmail.Should().Be("admin@northstar.io");
        tenant.IsArchived.Should().BeFalse();

        tenant.DomainEvents.Should().ContainSingle();
        var domainEvent = tenant.DomainEvents.First() as TenantCreatedDomainEvent;
        domainEvent.Should().NotBeNull();
        domainEvent!.TenantId.Should().Be(tenantId);
        domainEvent.Slug.Should().Be("northstar-digital");
        domainEvent.Name.Should().Be("Northstar Digital");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowDomainException(string? invalidName)
    {
        // Act
        var act = () => Tenant.Create(Guid.NewGuid(), invalidName!, "valid-slug");

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be("INVALID_TENANT_NAME");
    }

    [Fact]
    public void Create_WithNameExceeding200Chars_ShouldThrowDomainException()
    {
        // Arrange
        var longName = new string('A', 201);

        // Act
        var act = () => Tenant.Create(Guid.NewGuid(), longName, "valid-slug");

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be("TENANT_NAME_TOO_LONG");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptySlug_ShouldThrowDomainException(string? invalidSlug)
    {
        // Act
        var act = () => Tenant.Create(Guid.NewGuid(), "Valid Name", invalidSlug!);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be("INVALID_TENANT_SLUG");
    }

    [Theory]
    [InlineData("a")] // too short (< 2)
    [InlineData("-invalid")] // leading hyphen
    [InlineData("invalid-")] // trailing hyphen
    [InlineData("invalid slug")] // space
    [InlineData("invalid_slug")] // underscore
    [InlineData("invalid.slug")] // dot
    [InlineData("invalid@slug")] // special char
    public void Create_WithInvalidSlugFormat_ShouldThrowDomainException(string invalidSlug)
    {
        // Act
        var act = () => Tenant.Create(Guid.NewGuid(), "Valid Name", invalidSlug);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be("INVALID_TENANT_SLUG_FORMAT");
    }

    [Theory]
    [InlineData("northstar")]
    [InlineData("acme-corp")]
    [InlineData("tech-123")]
    [InlineData("ab")]
    public void Create_WithValidSlugFormats_ShouldSucceed(string validSlug)
    {
        // Act
        var tenant = Tenant.Create(Guid.NewGuid(), "Valid Name", validSlug);

        // Assert
        tenant.Slug.Should().Be(validSlug);
    }

    [Fact]
    public void Suspend_WithReason_ShouldChangeStatusAndRaiseEvent()
    {
        // Arrange
        var tenant = Tenant.Create(Guid.NewGuid(), "Tenant", "tenant-slug");
        tenant.ClearDomainEvents();

        // Act
        tenant.Suspend("Billing past due threshold");

        // Assert
        tenant.Status.Should().Be(TenantStatus.Suspended);
        tenant.DomainEvents.Should().ContainSingle();
        var statusEvent = tenant.DomainEvents.First() as TenantStatusChangedDomainEvent;
        statusEvent.Should().NotBeNull();
        statusEvent!.NewStatus.Should().Be(TenantStatus.Suspended);
        statusEvent.Reason.Should().Be("Billing past due threshold");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Suspend_WithoutReason_ShouldThrowDomainException(string? emptyReason)
    {
        // Arrange
        var tenant = Tenant.Create(Guid.NewGuid(), "Tenant", "tenant-slug");

        // Act
        var act = () => tenant.Suspend(emptyReason!);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be("SUSPENSION_REASON_REQUIRED");
    }

    [Fact]
    public void Activate_WhenSuspended_ShouldChangeStatusToActive()
    {
        // Arrange
        var tenant = Tenant.Create(Guid.NewGuid(), "Tenant", "tenant-slug");
        tenant.Suspend("Reason");
        tenant.ClearDomainEvents();

        // Act
        tenant.Activate();

        // Assert
        tenant.Status.Should().Be(TenantStatus.Active);
        tenant.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public void Archive_ShouldSetIsArchivedAndSuspend()
    {
        // Arrange
        var tenant = Tenant.Create(Guid.NewGuid(), "Tenant", "tenant-slug");
        var actorId = Guid.NewGuid();

        // Act
        tenant.Archive(actorId);

        // Assert
        tenant.IsArchived.Should().BeTrue();
        tenant.ArchivedAtUtc.Should().NotBeNull();
        tenant.ArchivedByUserId.Should().Be(actorId);
        tenant.Status.Should().Be(TenantStatus.Suspended);
    }
}
