using MediatR;
using SuperSafeBank.Common;
using SuperSafeBank.Common.EventBus;
using SuperSafeBank.Domain.IntegrationEvents;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSafeBank.Domain.Commands;

public record Transfer : IRequest
{
    public Transfer(Guid sourceAccountId, Guid destinationAccountId, Money amount)
    {
        SourceAccountId = sourceAccountId;
        DestinationAccountId = destinationAccountId;
        Amount = amount ?? throw new ArgumentNullException(nameof(amount));
    }
    public Guid SourceAccountId { get; }
    public Guid DestinationAccountId { get; }
    public Money Amount { get; }
}

public class TransferHandler : IRequestHandler<Transfer>
{
    private readonly IAggregateRepository<Account, Guid> _accountEventsService;
    private readonly IAggregateRepository<Transaction, Guid> _transactionRepo;
    private readonly IEventProducer _eventProducer;

    public TransferHandler(
        IAggregateRepository<Account, Guid> accountsRepo,
        IAggregateRepository<Transaction, Guid> transactionsRepo,
        IEventProducer eventProducer)
    {
        _accountEventsService = accountsRepo;
        _transactionRepo = transactionsRepo;
        _eventProducer = eventProducer;
    }

    public async Task Handle(Transfer command, CancellationToken cancellationToken)
    {
        var sourceAccount = await _accountEventsService.RehydrateAsync(command.SourceAccountId);
        if (null == sourceAccount)
            throw new ArgumentOutOfRangeException(nameof(Transfer.SourceAccountId), "invalid source account id");

        var destinationAccount = await _accountEventsService.RehydrateAsync(command.DestinationAccountId);
        if (null == destinationAccount)
            throw new ArgumentOutOfRangeException(nameof(Transfer.DestinationAccountId), "invalid destination account id");

        var transaction = Transaction.Transfer(
            sourceAccount,
            destinationAccount,
            command.Amount);

        await _transactionRepo.PersistAsync(transaction, cancellationToken)
                   .ConfigureAwait(false);

        var @event = new TransactionStarted(Guid.NewGuid(), transaction.Id);
        await _eventProducer.DispatchAsync(@event, cancellationToken)
                            .ConfigureAwait(false);
    }
}
