using CRMProAI.Domain.Common;
using FluentAssertions;
using Xunit;

namespace CRMProAI.Domain.UnitTests.Common;

public class EntityTests
{
    private class TestEntity : Entity<Guid>
    {
        public TestEntity(Guid id) : base(id) { }
    }

    private class AnotherTestEntity : Entity<Guid>
    {
        public AnotherTestEntity(Guid id) : base(id) { }
    }

    [Fact]
    public void Equals_WithSameIdAndType_ShouldReturnTrue()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        entity1.Equals(entity2).Should().BeTrue();
        (entity1 == entity2).Should().BeTrue();
        (entity1 != entity2).Should().BeFalse();
        entity1.GetHashCode().Should().Be(entity2.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentId_ShouldReturnFalse()
    {
        var entity1 = new TestEntity(Guid.NewGuid());
        var entity2 = new TestEntity(Guid.NewGuid());

        entity1.Equals(entity2).Should().BeFalse();
        (entity1 == entity2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentTypeSameId_ShouldReturnFalse()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new AnotherTestEntity(id);

        entity1.Equals(entity2).Should().BeFalse();
    }
}
