using FluentAssertions;
using System;
using Xunit;

namespace SuperSafeBank.Domain.Tests;

public class MoneyTests
{
    [Fact]
    public void Add_should_throw_when_currencies_do_not_match_and_converter_null()
    {
        var sut = Money.Zero(Currency.CanadianDollar);
        var other = Money.Zero(Currency.Euro);
        Assert.Throws<ArgumentNullException>(() => sut.Add(other));
    }

    [Fact]
    public void Subtract_should_throw_when_currencies_do_not_match_and_converter_null()
    {
        var sut = Money.Zero(Currency.CanadianDollar);
        var other = Money.Zero(Currency.Euro);
        Assert.Throws<ArgumentNullException>(() => sut.Subtract(other));
    }

    [Theory]
    [InlineData("42 CAD", 42, "CAD")]
    [InlineData("71 EUR", 71, "EUR")]
    [InlineData("16 USD", 16, "USD")]
    public void Parse_should_return_correct_value_when_input_valid(string s, decimal value, string currency)
    {
        var result = Money.Parse(s);
        result.Should().NotBeNull();
        result.Value.Should().Be(value);
        result.Currency.Name.Should().Be(currency);
    }
}