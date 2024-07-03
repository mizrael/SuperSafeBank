using System;
using System.Linq;
using System.Security.Principal;
using FluentAssertions;
using SuperSafeBank.Common.Models;
using SuperSafeBank.Domain.DomainEvents;
using SuperSafeBank.Domain.Services;
using Xunit;

namespace SuperSafeBank.Domain.Tests;

public class AccountTests
{
    [Fact]
    public void Create_should_create_valid_Account_instance()
    {
        var currencyConverter = new FakeCurrencyConverter();
        var customer = Customer.Create(Guid.NewGuid(), "lorem", "ipsum", "test@test.com");
        var account = Account.Create(Guid.NewGuid(), customer, Currency.CanadianDollar);
        account.Deposit(Transaction.Deposit(account, new Money(Currency.CanadianDollar, 10)), currencyConverter);
        account.Deposit(Transaction.Deposit(account, new Money(Currency.CanadianDollar, 42)), currencyConverter);
        account.Withdraw(Transaction.Withdraw(account, new Money(Currency.CanadianDollar, 4)), currencyConverter);
        account.Deposit(Transaction.Deposit(account, new Money(Currency.CanadianDollar, 71)), currencyConverter);

        var instance = BaseAggregateRoot<Account, Guid>.Create(account.Events);
        instance.Should().NotBeNull();
        instance.Id.Should().Be(account.Id);
        instance.OwnerId.Should().Be(customer.Id);
        instance.Balance.Should().NotBeNull();
        instance.Balance.Currency.Should().Be(Currency.CanadianDollar);
        instance.Balance.Value.Should().Be(account.Balance.Value);
    }

    [Fact]
    public void Create_should_return_valid_instance()
    {
        var customer = Customer.Create(Guid.NewGuid(), "lorem", "ipsum", "test@test.com");
        var sut = Account.Create(Guid.NewGuid(), customer, Currency.CanadianDollar);

        sut.Balance.Should().Be(Money.Zero(Currency.CanadianDollar));
        sut.OwnerId.Should().Be(customer.Id);
        sut.Version.Should().Be(1);
    }

    [Fact]
    public void Create_should_add_account_to_customer()
    {
        var customer = Customer.Create(Guid.NewGuid(), "lorem", "ipsum", "test@test.com");

        var account = Account.Create(Guid.NewGuid(), customer, Currency.CanadianDollar);
        customer.Accounts.Should().Contain(account.Id);
    }

    [Fact]
    public void ctor_should_raise_Created_event()
    {
        var customer = Customer.Create(Guid.NewGuid(), "lorem", "ipsum", "test@test.com");

        var accountId = Guid.NewGuid();
        var sut = new Account(accountId, customer, Currency.CanadianDollar);
        
        sut.Events.Count.Should().Be(1);

        var createdEvent = sut.Events.First() as AccountEvents.AccountCreated;
        createdEvent.Should().NotBeNull()
            .And.BeOfType<AccountEvents.AccountCreated>();
        createdEvent.AggregateId.Should().Be(accountId);
        createdEvent.AggregateVersion.Should().Be(0);
        createdEvent.OwnerId.Should().Be(customer.Id);
        createdEvent.Currency.Should().Be(Currency.CanadianDollar);
    }

    [Fact]
    public void Deposit_should_throw_when_transaction_invalid()
    {
        var customer = Customer.Create(Guid.NewGuid(), "lorem", "ipsum", "test@test.com");
        var sut = Account.Create(Guid.NewGuid(), customer, Currency.CanadianDollar);
        var currencyConverter = new FakeCurrencyConverter();

        Assert.Throws<ArgumentException>(() =>
            sut.Deposit(Transaction.Withdraw(sut, new Money(Currency.CanadianDollar, 1)), currencyConverter));
    }

    [Fact]
    public void Withdraw_should_throw_when_transaction_invalid()
    {
        var customer = Customer.Create(Guid.NewGuid(), "lorem", "ipsum", "test@test.com");
        var sut = Account.Create(Guid.NewGuid(), customer, Currency.CanadianDollar);
        var currencyConverter = new FakeCurrencyConverter();

        Assert.Throws<ArgumentException>(() =>
            sut.Withdraw(Transaction.Deposit(sut, new Money(Currency.CanadianDollar, 1)), currencyConverter));
    }

    [Fact]
    public void Deposit_should_add_value()
    {
        var customer = Customer.Create(Guid.NewGuid(), "lorem", "ipsum", "test@test.com");
        var sut = Account.Create(Guid.NewGuid(), customer, Currency.CanadianDollar);
        var currencyConverter = new FakeCurrencyConverter();

        sut.Balance.Should().Be(Money.Zero(Currency.CanadianDollar));

        sut.Deposit(Transaction.Deposit(sut, new Money(Currency.CanadianDollar, 1)), currencyConverter);
        sut.Balance.Should().Be(new Money(Currency.CanadianDollar, 1));
        sut.Version.Should().Be(2);

        sut.Deposit(Transaction.Deposit(sut, new Money(Currency.CanadianDollar, 9)), currencyConverter);
        sut.Balance.Should().Be(new Money(Currency.CanadianDollar, 10));
        sut.Version.Should().Be(3);
    }

    [Fact]
    public void Withdraw_should_throw_if_current_balance_is_below_amount()
    {
        var customer = Customer.Create(Guid.NewGuid(), "lorem", "ipsum", "test@test.com");
        var sut = Account.Create(Guid.NewGuid(), customer, Currency.CanadianDollar);
        var currencyConverter = new FakeCurrencyConverter();

        sut.Balance.Should().Be(Money.Zero(Currency.CanadianDollar));
        
        Assert.Throws<AccountTransactionException>(() =>
            sut.Withdraw(Transaction.Withdraw(sut, new Money(Currency.CanadianDollar, 1)), currencyConverter));
    }

    [Fact]
    public void Withdraw_should_remove_value()
    {
        var customer = Customer.Create(Guid.NewGuid(), "lorem", "ipsum", "test@test.com");
        var sut = Account.Create(Guid.NewGuid(), customer, Currency.CanadianDollar);
        var currencyConverter = new FakeCurrencyConverter();

        sut.Deposit(Transaction.Deposit(sut, new Money(Currency.CanadianDollar, 10)), currencyConverter);
        sut.Withdraw(Transaction.Withdraw(sut, new Money(Currency.CanadianDollar, 1)), currencyConverter);

        sut.Balance.Should().Be(new Money(Currency.CanadianDollar, 9));
        sut.Version.Should().Be(3);
    }
}
