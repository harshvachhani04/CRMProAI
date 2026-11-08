using CRMProAI.Infrastructure.Persistence;
using FluentAssertions;
using Xunit;

namespace CRMProAI.Infrastructure.IntegrationTests;

public class InfrastructureSanityTests
{
    [Fact]
    public void InfrastructureAssembly_ShouldLoadSuccessfully()
    {
        var infraAssembly = typeof(InfrastructureMarker).Assembly;
        infraAssembly.Should().NotBeNull();
        infraAssembly.GetName().Name.Should().Be("CRMProAI.Infrastructure");
    }
}
