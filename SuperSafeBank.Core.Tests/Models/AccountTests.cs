using System;
using System.Linq;
using FluentAssertions;
using SuperSafeBank.Core.Models;
using SuperSafeBank.Core.Models.Events;
using SuperSafeBank.Core.Services;
using Xunit;

namespace SuperSafeBank.Core.Tests.Models
{
    public class AccountTests
    {
        [Fact]
        public void ctor_should_create_valid_instance()
        {
            var customer = Customer.Create("lorem", "ipsum");
            var sut = Account.Create(customer, Currency.CanadianDollar);

            sut.Balance.Should().Be(Money.Zero(Currency.CanadianDollar));
            sut.OwnerId.Should().Be(customer.Id);
            sut.Version.Should().Be(1);
        }

        [Fact]
        public void ctor_should_raise_Created_event()
        {
            var customer = Customer.Create("lorem", "ipsum");

            var accountId = Guid.NewGuid();
            var sut = new Account(accountId, customer, Currency.CanadianDollar);
            
            sut.Events.Count.Should().Be(1);

            var createdEvent = sut.Events.First() as AccountCreated;
            createdEvent.Should().NotBeNull()
                .And.BeOfType<AccountCreated>();
            createdEvent.AggregateId.Should().Be(accountId);
            createdEvent.AggregateVersion.Should().Be(0);
            createdEvent.OwnerId.Should().Be(customer.Id);
            createdEvent.Currency.Should().Be(Currency.CanadianDollar);
        }

        [Fact]
        public void Deposit_should_add_value()
        {
            var customer = Customer.Create("lorem", "ipsum");
            var sut = Account.Create(customer, Currency.CanadianDollar);
            var currencyConverter = new FakeCurrencyConverter();

            sut.Balance.Should().Be(Money.Zero(Currency.CanadianDollar));
           
            sut.Deposit(new Money(Currency.CanadianDollar, 1), currencyConverter);
            sut.Balance.Should().Be(new Money(Currency.CanadianDollar, 1));
            sut.Version.Should().Be(2);

            sut.Deposit(new Money(Currency.CanadianDollar, 9), currencyConverter);
            sut.Balance.Should().Be(new Money(Currency.CanadianDollar, 10));
            sut.Version.Should().Be(3);
        }

        [Fact]
        public void Withdraw_should_throw_if_current_balance_is_below_amount()
        {
            var customer = Customer.Create("lorem", "ipsum");
            var sut = Account.Create(customer, Currency.CanadianDollar);
            var currencyConverter = new FakeCurrencyConverter();

            sut.Balance.Should().Be(Money.Zero(Currency.CanadianDollar));
            
            Assert.Throws<AccountTransactionException>(() => sut.Withdraw(new Money(Currency.CanadianDollar, 1), currencyConverter));
        }

        [Fact]
        public void Withdraw_should_remove_value()
        {
            var customer = Customer.Create("lorem", "ipsum");
            var sut = Account.Create(customer, Currency.CanadianDollar);
            var currencyConverter = new FakeCurrencyConverter();

            sut.Deposit(new Money(Currency.CanadianDollar, 10), currencyConverter);

            sut.Withdraw(new Money(Currency.CanadianDollar, 1), currencyConverter);

            sut.Balance.Should().Be(new Money(Currency.CanadianDollar, 9));
            sut.Version.Should().Be(3);
        }
    }
}
