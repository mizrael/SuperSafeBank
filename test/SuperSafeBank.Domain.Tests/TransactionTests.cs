using FluentAssertions;
using System;
using Xunit;

namespace SuperSafeBank.Domain.Tests;

public partial class TransactionTests
{
    [Fact]
    public void Transfer_should_create_transaction()
    {
        var sourceAccount = Guid.NewGuid();
        var destinationAccount = Guid.NewGuid();
        var amount = Money.Zero(Currency.CanadianDollar);

        var sut = Transaction.Transfer(sourceAccount, destinationAccount, amount);

        Assert.NotNull(sut);
        Assert.Equal("Transfer", sut.Type);
        Assert.Equal(3, sut.States.Length);
        Assert.Equal("Pending", sut.States[0]);
        Assert.Equal("Withdrawn", sut.States[1]);
        Assert.Equal("Deposited", sut.States[2]);
        Assert.Equal(sourceAccount.ToString(), sut.Properties["SourceAccount"]);
        Assert.Equal(destinationAccount.ToString(), sut.Properties["DestinationAccount"]);
        Assert.Equal(amount.ToString(), sut.Properties["Amount"]);
    }

    [Fact]
    public void StepForward_should_move_transaction_state()
    {
        var transaction = Transaction.Transfer(Guid.NewGuid(), Guid.NewGuid(), Money.Zero(Currency.CanadianDollar));
        transaction.CurrentState.Should().NotBeNullOrWhiteSpace();

        transaction.StepForward();
        transaction.CurrentState.Should().Be(TransactionTypes.TransferStates[0]);

        transaction.StepForward();
        transaction.CurrentState.Should().Be(TransactionTypes.TransferStates[1]);

        transaction.StepForward();
        transaction.CurrentState.Should().Be(TransactionTypes.TransferStates[2]);
    }
}
