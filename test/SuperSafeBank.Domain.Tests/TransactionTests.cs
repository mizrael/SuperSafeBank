using FluentAssertions;
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

        Assert.NotNull(sut);
        Assert.Equal("Transfer", sut.Type);
        Assert.Equal(3, sut.States.Length);
        Assert.Equal("Pending", sut.States[0]);
        Assert.Equal("Withdrawn", sut.States[1]);
        Assert.Equal("Deposited", sut.States[2]);
        Assert.Equal(sourceAccount.Id.ToString(), sut.Properties["SourceAccount"]);
        Assert.Equal(destinationAccount.Id.ToString(), sut.Properties["DestinationAccount"]);
        Assert.Equal(amount.ToString(), sut.Properties["Amount"]);
    }

    [Fact]
    public void Deposit_should_create_transaction()
    {
        var customer = Customer.Create(Guid.NewGuid(), "john", "doe", "test@test.com");
        var destinationAccount = new Account(Guid.NewGuid(), customer, Currency.CanadianDollar);
        var amount = Money.Zero(Currency.CanadianDollar);

        var sut = Transaction.Deposit(destinationAccount, amount);

        Assert.NotNull(sut);
        Assert.Equal("Deposit", sut.Type);
        Assert.Equal(2, sut.States.Length);
        Assert.Equal("Pending", sut.States[0]);
        Assert.Equal("Deposited", sut.States[1]);
        Assert.Equal(destinationAccount.Id.ToString(), sut.Properties["DestinationAccount"]);
        Assert.Equal(amount.ToString(), sut.Properties["Amount"]);
    }

    [Fact]
    public void Withdraw_should_create_transaction()
    {
        var customer = Customer.Create(Guid.NewGuid(), "john", "doe", "test@test.com");
        var account = new Account(Guid.NewGuid(), customer, Currency.CanadianDollar);
        var amount = Money.Zero(Currency.CanadianDollar);

        var sut = Transaction.Withdraw(account, amount);

        Assert.NotNull(sut);
        Assert.Equal("Withdraw", sut.Type);
        Assert.Equal(2, sut.States.Length);
        Assert.Equal("Pending", sut.States[0]);
        Assert.Equal("Withdrawn", sut.States[1]);
        Assert.Equal(account.Id.ToString(), sut.Properties["SourceAccount"]);
        Assert.Equal(amount.ToString(), sut.Properties["Amount"]);
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

        sut.StepForward();
        sut.CurrentState.Should().Be(TransactionTypes.TransferStates[2]);
    }
}
