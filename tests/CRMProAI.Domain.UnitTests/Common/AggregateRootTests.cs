using CRMProAI.Domain.Common;
using FluentAssertions;
using Xunit;

namespace CRMProAI.Domain.UnitTests.Common;

public class AggregateRootTests
{
    private record SampleDomainEvent(string Message) : IDomainEvent;

    private class SampleAggregate : AggregateRoot<Guid>
    {
        public SampleAggregate(Guid id) : base(id) { }

        public void TriggerAction(string message)
        {
            AddDomainEvent(new SampleDomainEvent(message));
        }
    }

    [Fact]
    public void AddDomainEvent_ShouldAppendEventToDomainEventsCollection()
    {
        var aggregate = new SampleAggregate(Guid.NewGuid());

        aggregate.TriggerAction("OrderCreated");

        aggregate.DomainEvents.Should().HaveCount(1);
        aggregate.DomainEvents.First().Should().BeOfType<SampleDomainEvent>();
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmptyTheCollection()
    {
        var aggregate = new SampleAggregate(Guid.NewGuid());
        aggregate.TriggerAction("FirstEvent");
        aggregate.TriggerAction("SecondEvent");

        aggregate.DomainEvents.Should().HaveCount(2);

        aggregate.ClearDomainEvents();

        aggregate.DomainEvents.Should().BeEmpty();
    }
}
