using CRMProAI.Domain.Common;
using FluentAssertions;
using Xunit;

namespace CRMProAI.Domain.UnitTests.Common;

public class ValueObjectTests
{
    private class Money : ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }

    [Fact]
    public void Equals_WithIdenticalComponents_ShouldReturnTrue()
    {
        var m1 = new Money(100.50m, "USD");
        var m2 = new Money(100.50m, "USD");

        m1.Equals(m2).Should().BeTrue();
        (m1 == m2).Should().BeTrue();
        m1.GetHashCode().Should().Be(m2.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentAmountOrCurrency_ShouldReturnFalse()
    {
        var m1 = new Money(100m, "USD");
        var m2 = new Money(100m, "EUR");
        var m3 = new Money(200m, "USD");

        (m1 == m2).Should().BeFalse();
        (m1 == m3).Should().BeFalse();
    }
}
