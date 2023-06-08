using System;
using Xunit;

namespace SuperSafeBank.Domain.Tests
{
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
    }
}