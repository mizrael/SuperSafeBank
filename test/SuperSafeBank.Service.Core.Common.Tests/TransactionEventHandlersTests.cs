using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SuperSafeBank.Common;
using SuperSafeBank.Common.EventBus;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.IntegrationEvents;
using SuperSafeBank.Domain.Services;
using SuperSafeBank.Service.Core.EventHandlers;

namespace SuperSafeBank.Service.Core.Common.Tests;

public class TransactionEventHandlersTests
{
    [Fact]
    public async Task Handle_should_process_Withdrawn_when_status_empty()
    {
        var currencyConverter = Substitute.For<ICurrencyConverter>();
        currencyConverter.Convert(Arg.Any<Money>(), Arg.Any<Currency>())
                         .Returns(args => args[0]);
        var customer = Customer.Create(Guid.NewGuid(), "john", "doe", "test@test.com");
        var sourceAccount = new Account(Guid.NewGuid(), customer, Currency.CanadianDollar);
        var deposit = Transaction.Deposit(sourceAccount, Money.Parse("100 CAD"));
        sourceAccount.Deposit(deposit, currencyConverter);

        var transaction = Transaction.Withdraw(sourceAccount, Money.Parse("50 CAD"), currencyConverter);
        var @event = new TransactionStarted(Guid.NewGuid(), transaction.Id);

        var transactionsRepo = Substitute.For<IAggregateRepository<Transaction, Guid>>();
        transactionsRepo.RehydrateAsync(transaction.Id, Arg.Any<CancellationToken>())
                        .Returns(transaction);

        var accountsRepo = Substitute.For<IAggregateRepository<Account, Guid>>();
        accountsRepo.RehydrateAsync(sourceAccount.Id, Arg.Any<CancellationToken>())
                    .Returns(sourceAccount);
        var logger = Substitute.For<ILogger<TransactionEventHandlers>>();

        var eventProducer = Substitute.For<IEventProducer>();
        var sut = new TransactionEventHandlers(
            transactionsRepo,
            accountsRepo,
            logger,
            currencyConverter,
            eventProducer);

        await sut.Handle(@event, CancellationToken.None);

        transaction.CurrentState.Should().Be("Withdrawn");
        transaction.IsCompleted.Should().BeTrue();

        sourceAccount.Balance.Value.Should().Be(50);
    }

    [Fact]
    public async Task Handle_should_process_Deposit_when_status_empty()
    {
        var currencyConverter = Substitute.For<ICurrencyConverter>();
        currencyConverter.Convert(Arg.Any<Money>(), Arg.Any<Currency>())
                         .Returns(args => args[0]);
        var customer = Customer.Create(Guid.NewGuid(), "john", "doe", "test@test.com");
        var destinationAccount = new Account(Guid.NewGuid(), customer, Currency.CanadianDollar);

        var transaction = Transaction.Deposit(destinationAccount, Money.Parse("50 CAD"));
        var @event = new TransactionStarted(Guid.NewGuid(), transaction.Id);

        var transactionsRepo = Substitute.For<IAggregateRepository<Transaction, Guid>>();
        transactionsRepo.RehydrateAsync(transaction.Id, Arg.Any<CancellationToken>())
                        .Returns(transaction);

        var accountsRepo = Substitute.For<IAggregateRepository<Account, Guid>>();
        accountsRepo.RehydrateAsync(destinationAccount.Id, Arg.Any<CancellationToken>())
                    .Returns(destinationAccount);
        var logger = Substitute.For<ILogger<TransactionEventHandlers>>();

        var eventProducer = Substitute.For<IEventProducer>();
        var sut = new TransactionEventHandlers(
            transactionsRepo,
            accountsRepo,
            logger,
            currencyConverter,
            eventProducer);

        await sut.Handle(@event, CancellationToken.None);

        transaction.CurrentState.Should().Be("Deposited");
        transaction.IsCompleted.Should().BeTrue();

        destinationAccount.Balance.Value.Should().Be(50);
    }

    [Fact]
    public async Task Handle_should_process_Transfer_when_status_empty()
    {
        var currencyConverter = Substitute.For<ICurrencyConverter>();
        currencyConverter.Convert(Arg.Any<Money>(), Arg.Any<Currency>())
                         .Returns(args => args[0]);

        var customer = Customer.Create(Guid.NewGuid(), "john", "doe", "test@test.com");
        var sourceAccount = new Account(Guid.NewGuid(), customer, Currency.CanadianDollar);
        
        var deposit = Transaction.Deposit(sourceAccount, Money.Parse("100 CAD"));
        sourceAccount.Deposit(deposit, currencyConverter);

        var destinationAccount = new Account(Guid.NewGuid(), customer, Currency.CanadianDollar);

        var transaction = Transaction.Transfer(sourceAccount, destinationAccount, Money.Parse("50 CAD"));
        var @event = new TransactionStarted(Guid.NewGuid(), transaction.Id);

        var transactionsRepo = NSubstitute.Substitute.For<IAggregateRepository<Transaction, Guid>>();
        transactionsRepo.RehydrateAsync(transaction.Id, Arg.Any<CancellationToken>())
                        .Returns(transaction);

        var accountsRepo = Substitute.For<IAggregateRepository<Account, Guid>>();
        accountsRepo.RehydrateAsync(sourceAccount.Id, Arg.Any<CancellationToken>())
                    .Returns(sourceAccount);

        var logger = Substitute.For<ILogger<TransactionEventHandlers>>();        

        var eventProducer = Substitute.For<IEventProducer>();
        var sut = new TransactionEventHandlers(
            transactionsRepo,
            accountsRepo,
            logger,
            currencyConverter,
            eventProducer);

        await sut.Handle(@event, CancellationToken.None);

        transaction.CurrentState.Should().Be("Withdrawn");

        sourceAccount.Balance.Value.Should().Be(50);
    }

    [Fact]
    public async Task Handle_should_process_finalizing_Transfer()
    {
        var currencyConverter = Substitute.For<ICurrencyConverter>();
        currencyConverter.Convert(Arg.Any<Money>(), Arg.Any<Currency>())
                         .Returns(args => args[0]);

        var customer = Customer.Create(Guid.NewGuid(), "john", "doe", "test@test.com");
        var sourceAccount = new Account(Guid.NewGuid(), customer, Currency.CanadianDollar);
        var destinationAccount = new Account(Guid.NewGuid(), customer, Currency.CanadianDollar);

        var transaction = Transaction.Transfer(sourceAccount, destinationAccount, Money.Parse("50 CAD"));
        transaction.StepForward();
        var @event = new TransactionStarted(Guid.NewGuid(), transaction.Id);

        var transactionsRepo = NSubstitute.Substitute.For<IAggregateRepository<Transaction, Guid>>();
        transactionsRepo.RehydrateAsync(transaction.Id, Arg.Any<CancellationToken>())
                        .Returns(transaction);

        var accountsRepo = Substitute.For<IAggregateRepository<Account, Guid>>();
        accountsRepo.RehydrateAsync(destinationAccount.Id, Arg.Any<CancellationToken>())
                    .Returns(destinationAccount);

        var logger = Substitute.For<ILogger<TransactionEventHandlers>>();


        var eventProducer = Substitute.For<IEventProducer>();
        var sut = new TransactionEventHandlers(
            transactionsRepo,
            accountsRepo,
            logger,
            currencyConverter,
            eventProducer);

        await sut.Handle(@event, CancellationToken.None);

        transaction.CurrentState.Should().Be("Deposited");
        transaction.IsCompleted.Should().BeTrue();

        destinationAccount.Balance.Value.Should().Be(50);
    }
}