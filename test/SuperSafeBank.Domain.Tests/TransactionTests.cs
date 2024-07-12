using FluentAssertions;
using NSubstitute;
using SuperSafeBank.Domain.Services;
using System;
using Xunit;

namespace SuperSafeBank.Domain.Tests;

public partial class TransactionTests
{
    [Fact]
    public void Transfer_should_create_transaction()
    {
        var customer = Customer.Create(Guid.NewGuid(), "john", "doe", "test@test.com");
        var sourceAccount = new Account(Guid.NewGuid(), customer, Currency.CanadianDollar);
        var destinationAccount = new Account(Guid.NewGuid(), customer, Currency.CanadianDollar);
        var amount = Money.Zero(Currency.CanadianDollar);

        var sut = Transaction.Transfer(sourceAccount, destinationAccount, amount);

        sut.Should().NotBeNull();
        sut.Type.Should().Be("Transfer");
        sut.States.Should().HaveCount(2);
        sut.States[0].Should().Be("Withdrawn");
        sut.States[1].Should().Be("Deposited");

        sut.Properties.Should().ContainKey("SourceAccount")
            .WhoseValue.Should().Be(sourceAccount.Id.ToString());
        sut.Properties.Should().ContainKey("DestinationAccount")
            .WhoseValue.Should().Be(destinationAccount.Id.ToString());
        sut.Properties.Should().ContainKey("Amount")
            .WhoseValue.Should().Be(amount.ToString());
    }

    [Fact]
    public void Deposit_should_create_transaction()
    {
        var customer = Customer.Create(Guid.NewGuid(), "john", "doe", "test@test.com");
        var destinationAccount = new Account(Guid.NewGuid(), customer, Currency.CanadianDollar);
        var amount = Money.Zero(Currency.CanadianDollar);

        var sut = Transaction.Deposit(destinationAccount, amount);
        sut.Should().NotBeNull();
        sut.Type.Should().Be("Deposit");
        sut.CurrentState.Should().BeNullOrEmpty();
        sut.States.Should().HaveCount(1);
        sut.States[0].Should().Be("Deposited");
        sut.Properties.Should().ContainKey("DestinationAccount")
            .WhoseValue.Should().Be(destinationAccount.Id.ToString());
        sut.Properties.Should().ContainKey("Amount")
            .WhoseValue.Should().Be(amount.ToString());
    }

    [Fact]
    public void Withdraw_should_create_transaction()
    {
        var customer = Customer.Create(Guid.NewGuid(), "john", "doe", "test@test.com");
        var account = new Account(Guid.NewGuid(), customer, Currency.CanadianDollar);
        var amount = Money.Zero(Currency.CanadianDollar);

        var currencyConverter = Substitute.For<ICurrencyConverter>();
        currencyConverter.Convert(amount, account.Balance.Currency)
                         .Returns(amount);

        var sut = Transaction.Withdraw(account, amount, currencyConverter);

        sut.Should().NotBeNull();
        sut.Type.Should().Be("Withdraw");
        sut.CurrentState.Should().BeNullOrEmpty();
        sut.States.Should().HaveCount(1);
        sut.States[0].Should().Be("Withdrawn");
        sut.Properties.Should().ContainKey("SourceAccount")
            .WhoseValue.Should().Be(account.Id.ToString());
        sut.Properties.Should().ContainKey("Amount")
            .WhoseValue.Should().Be(amount.ToString());
    }

    [Fact]
    public void Withdraw_should_throw_if_less_than_zero()
    {
        var customer = Customer.Create(Guid.NewGuid(), "john", "doe", "test@test.com");
        var account = new Account(Guid.NewGuid(), customer, Currency.CanadianDollar);
        var amount = Money.Parse("-42, CAD");
        var currencyConverter = Substitute.For<ICurrencyConverter>();

        Assert.Throws<ArgumentOutOfRangeException>(() => Transaction.Withdraw(account, amount, currencyConverter));
    }

    [Fact]
    public void Withdraw_should_throw_if_amount_too_big()
    {
        var customer = Customer.Create(Guid.NewGuid(), "john", "doe", "test@test.com");
        var account = new Account(Guid.NewGuid(), customer, Currency.CanadianDollar);
        var amount = Money.Parse("42, CAD");

        var currencyConverter = Substitute.For<ICurrencyConverter>();
        currencyConverter.Convert(amount, account.Balance.Currency)
                        .Returns(amount);

        Assert.Throws<AccountTransactionException>(() => Transaction.Withdraw(account, amount, currencyConverter));
    }

    [Fact]
    public void StepForward_should_move_transaction_state()
    {
        var customer = Customer.Create(Guid.NewGuid(), "john", "doe", "test@test.com");
        var sourceAccount = new Account(Guid.NewGuid(), customer, Currency.CanadianDollar);
        var destinationAccount = new Account(Guid.NewGuid(), customer, Currency.CanadianDollar);
        var amount = Money.Zero(Currency.CanadianDollar);
        var sut = Transaction.Transfer(sourceAccount, destinationAccount, amount);
        sut.CurrentState.Should().BeNullOrWhiteSpace();

        sut.StepForward();
        sut.CurrentState.Should().Be(TransactionTypes.TransferStates[0]);

        sut.StepForward();
        sut.CurrentState.Should().Be(TransactionTypes.TransferStates[1]);

        sut.IsCompleted.Should().BeTrue();
    }
}
