using MediatR;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Common;
using SuperSafeBank.Common.EventBus;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.IntegrationEvents;
using SuperSafeBank.Domain.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSafeBank.Service.Core.EventHandlers;

public class TransactionEventHandlers : INotificationHandler<TransactionStarted>
{
    private readonly IAggregateRepository<Transaction, Guid> _transactionsRepo;
    private readonly IAggregateRepository<Account, Guid> _accountsRepo;
    private readonly ICurrencyConverter _currencyConverter;
    private readonly IEventProducer _eventProducer;
    private readonly ILogger<TransactionEventHandlers> _logger;

    public TransactionEventHandlers(
        IAggregateRepository<Transaction, Guid> transactionsRepo,
        IAggregateRepository<Account, Guid> accountsRepo,
        ILogger<TransactionEventHandlers> logger,
        ICurrencyConverter currencyConverter,
        IEventProducer eventProducer)
    {
        _transactionsRepo = transactionsRepo ?? throw new ArgumentNullException(nameof(transactionsRepo));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _accountsRepo = accountsRepo ?? throw new ArgumentNullException(nameof(accountsRepo));
        _currencyConverter = currencyConverter ?? throw new ArgumentNullException(nameof(currencyConverter));
        _eventProducer = eventProducer ?? throw new ArgumentNullException(nameof(eventProducer));
    }

    public async Task Handle(TransactionStarted @event, CancellationToken cancellationToken)
    {
        var transaction = await _transactionsRepo.RehydrateAsync(@event.TransactionId, cancellationToken)
                                                 .ConfigureAwait(false);
        if (transaction is null)
            throw new ArgumentOutOfRangeException(nameof(@event.TransactionId), $"Invalid transaction id: '{@event.TransactionId}'");

        _logger.LogInformation("handling transaction {transactionId} of type {transactionType}...", transaction.Id, transaction.Type);

        if (transaction.IsCompleted)
        {
            _logger.LogInformation("transaction {transactionId} already completed", transaction.Id);
            return;
        }

        switch (transaction.Type)
        {
            case TransactionTypes.Withdraw:
                await HandleWihdrawAsync(transaction, cancellationToken).ConfigureAwait(false);
                break;
            case TransactionTypes.Deposit:
                await HandleDepositAsync(transaction, cancellationToken).ConfigureAwait(false);
                break;
            case TransactionTypes.Transfer:
                await HandleTransferAsync(transaction, cancellationToken).ConfigureAwait(false);
                break;
            default:
                throw new NotImplementedException($"transaction type '{transaction.Type}' not implemented");
        }
    }

    private async Task HandleDepositAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        if (!transaction.TryGetAmount(out var amount))
            throw new ArgumentException("missing Amount");
        if (!transaction.TryGetDestinationAccountId(out var destinationAccountId))
            throw new ArgumentException("missing Destination Account");

        if (string.IsNullOrWhiteSpace(transaction.CurrentState))
        {
            _logger.LogInformation("transaction {transactionId}: depositing {amount} into account {destinationAccountId}", transaction.Id, amount, destinationAccountId);

            var destinationAccount = await _accountsRepo.RehydrateAsync(destinationAccountId, CancellationToken.None)
                                                           .ConfigureAwait(false);
            if (destinationAccount is null)
                throw new InvalidOperationException($"destination account {destinationAccountId} not found");

            destinationAccount.Deposit(transaction, _currencyConverter);
            await _accountsRepo.PersistAsync(destinationAccount, cancellationToken)
                               .ConfigureAwait(false);
        }

        transaction.StepForward();
        await _transactionsRepo.PersistAsync(transaction, cancellationToken)
                               .ConfigureAwait(false);
    }

    private async Task HandleWihdrawAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        if (!transaction.TryGetAmount(out var amount))
            throw new ArgumentException("missing Amount");
        if (!transaction.TryGetSourceAccountId(out var sourceAccountId))
            throw new ArgumentException("missing Source Account");

        if (string.IsNullOrWhiteSpace(transaction.CurrentState))
        {
            _logger.LogInformation("transaction {transactionId}: withdrawing {amount} from account {sourceAccountId}", transaction.Id, amount, sourceAccountId);

            var sourceAccount = await _accountsRepo.RehydrateAsync(sourceAccountId, CancellationToken.None)
                                                      .ConfigureAwait(false);
            if (sourceAccount is null)
                throw new InvalidOperationException($"source account {sourceAccountId} not found");

            sourceAccount.Withdraw(transaction, _currencyConverter);
            await _accountsRepo.PersistAsync(sourceAccount, cancellationToken)
                               .ConfigureAwait(false);
        }

        transaction.StepForward();
        await _transactionsRepo.PersistAsync(transaction, cancellationToken)
                               .ConfigureAwait(false);
    }

    private async Task HandleTransferAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        if(!transaction.TryGetAmount(out var amount))
            throw new ArgumentException("missing Amount");
        if(!transaction.TryGetSourceAccountId(out var sourceAccountId))
            throw new ArgumentException("missing Source Account");
        if(!transaction.TryGetDestinationAccountId(out var destinationAccountId))
            throw new ArgumentException("missing Destination Account");

        //TODO: outbox

        if (string.IsNullOrWhiteSpace(transaction.CurrentState))
        {
            _logger.LogInformation("transaction {transactionId}: withdrawing {amount} from account {sourceAccountId}", transaction.Id, amount, sourceAccountId);

            var sourceAccount = await _accountsRepo.RehydrateAsync(sourceAccountId, CancellationToken.None)
                                                      .ConfigureAwait(false);
            if (sourceAccount is null)
                throw new InvalidOperationException($"source account {sourceAccountId} not found");

            sourceAccount.Withdraw(transaction, _currencyConverter);
            await _accountsRepo.PersistAsync(sourceAccount, cancellationToken)
                               .ConfigureAwait(false);
        }
        else if (transaction.CurrentState == "Withdrawn")
        {
            _logger.LogInformation("transaction {transactionId}: depositing {amount} into account {destinationAccountId}", transaction.Id, amount, destinationAccountId);

            var destinationAccount = await _accountsRepo.RehydrateAsync(destinationAccountId, CancellationToken.None)
                                                           .ConfigureAwait(false);
            if (destinationAccount is null)
                throw new InvalidOperationException($"destination account {destinationAccountId} not found");
            
            destinationAccount.Deposit(transaction, _currencyConverter);
            await _accountsRepo.PersistAsync(destinationAccount, cancellationToken)
                               .ConfigureAwait(false);
        }

        transaction.StepForward();
        await _transactionsRepo.PersistAsync(transaction, cancellationToken)
                               .ConfigureAwait(false);
    }
}