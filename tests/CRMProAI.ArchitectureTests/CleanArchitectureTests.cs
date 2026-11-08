using System.Reflection;
using CRMProAI.Domain.Common;
using CRMProAI.Infrastructure.Persistence;
using FluentAssertions;
using Xunit;

namespace CRMProAI.ArchitectureTests;

public class CleanArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(ITenantScoped).Assembly;
    private static readonly Assembly ContractsAssembly = typeof(CRMProAI.Contracts.Common.PagedList<>).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(CRMProAI.Application.Common.ITenantContext).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(InfrastructureMarker).Assembly;
    private static readonly Assembly ApiAssembly = typeof(Program).Assembly;

    [Fact]
    public void DomainAssembly_ShouldNotReferenceOtherApplicationLayers()
    {
        var referencedAssemblies = DomainAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        referencedAssemblies.Should().NotContain("CRMProAI.Application", "Domain must not depend on Application layer.");
        referencedAssemblies.Should().NotContain("CRMProAI.Infrastructure", "Domain must not depend on Infrastructure layer.");
        referencedAssemblies.Should().NotContain("CRMProAI.Api", "Domain must not depend on Api layer.");
        referencedAssemblies.Should().NotContain("CRMProAI.Worker", "Domain must not depend on Worker layer.");
    }

    [Fact]
    public void ContractsAssembly_ShouldNotReferenceInternalApplicationLayers()
    {
        var referencedAssemblies = ContractsAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        referencedAssemblies.Should().NotContain("CRMProAI.Domain", "Contracts must be standalone.");
        referencedAssemblies.Should().NotContain("CRMProAI.Application", "Contracts must be standalone.");
        referencedAssemblies.Should().NotContain("CRMProAI.Infrastructure", "Contracts must be standalone.");
        referencedAssemblies.Should().NotContain("CRMProAI.Api", "Contracts must be standalone.");
    }

    [Fact]
    public void ApplicationAssembly_ShouldNotReferenceInfrastructureOrApi()
    {
        var referencedAssemblies = ApplicationAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        referencedAssemblies.Should().NotContain("CRMProAI.Infrastructure", "Application must not depend on Infrastructure.");
        referencedAssemblies.Should().NotContain("CRMProAI.Api", "Application must not depend on Api.");
        referencedAssemblies.Should().NotContain("CRMProAI.Worker", "Application must not depend on Worker.");
    }

    [Fact]
    public void InfrastructureAssembly_ShouldReferenceApplicationAndDomain()
    {
        var referencedAssemblies = InfrastructureAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        referencedAssemblies.Should().Contain("CRMProAI.Domain", "Infrastructure must implement Domain persistence.");
        referencedAssemblies.Should().Contain("CRMProAI.Application", "Infrastructure must implement Application ports.");
    }
}
