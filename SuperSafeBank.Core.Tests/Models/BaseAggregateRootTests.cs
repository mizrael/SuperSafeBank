using System;
using FluentAssertions;
using SuperSafeBank.Core.Models;
using SuperSafeBank.Core.Services;
using Xunit;

namespace SuperSafeBank.Core.Tests.Models
{
    public class BaseAggregateRootTests
    {
        [Fact]
        public void Create_should_create_valid_Customer_instance()
        {
            var customer = new Customer(Guid.NewGuid(), "lorem", "ipsum");
            
            var instance = BaseAggregateRoot<Customer, Guid>.Create(customer.Events);
            instance.Should().NotBeNull();
            instance.Id.Should().Be(customer.Id);
            instance.Firstname.Should().Be(customer.Firstname);
            instance.Lastname.Should().Be(customer.Lastname);
        }

        [Fact]
        public void Create_should_create_valid_Account_instance()
        {
            var currencyConverter = new FakeCurrencyConverter();
            var customer = new Customer(Guid.NewGuid(), "lorem", "ipsum");
            var account = Account.Create(customer, Currency.CanadianDollar);
            account.Deposit(new Money(Currency.CanadianDollar, 10), currencyConverter);
            account.Deposit(new Money(Currency.CanadianDollar, 42), currencyConverter);
            account.Withdraw(new Money(Currency.CanadianDollar, 4), currencyConverter);
            account.Deposit(new Money(Currency.CanadianDollar, 71), currencyConverter);

            var instance = BaseAggregateRoot<Account, Guid>.Create(account.Events);
            instance.Should().NotBeNull();
            instance.Id.Should().Be(account.Id);
            instance.OwnerId.Should().Be(customer.Id);
            instance.Balance.Should().NotBeNull();
            instance.Balance.Currency.Should().Be(Currency.CanadianDollar);
            instance.Balance.Value.Should().Be(account.Balance.Value);
        }
    }
}