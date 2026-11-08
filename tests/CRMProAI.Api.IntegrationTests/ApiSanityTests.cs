using FluentAssertions;
using Xunit;

namespace CRMProAI.Api.IntegrationTests;

public class ApiSanityTests
{
    [Fact]
    public void ApiAssembly_ShouldLoadSuccessfully()
    {
        var apiAssembly = typeof(Program).Assembly;
        apiAssembly.Should().NotBeNull();
        apiAssembly.GetName().Name.Should().Be("CRMProAI.Api");
    }
}
