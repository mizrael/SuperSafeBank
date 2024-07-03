using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SuperSafeBank.Common;
using SuperSafeBank.Common.EventBus;
using SuperSafeBank.Domain.IntegrationEvents;
using SuperSafeBank.Domain.Services;

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
    private readonly IAggregateRepository<Transaction, Guid> _repo;
    private readonly ICurrencyConverter _currencyConverter;
    private readonly IEventProducer _eventProducer;

    public TransferHandler(IAggregateRepository<Transaction, Guid> repo, ICurrencyConverter currencyConverter, IEventProducer eventProducer)
    {
        _repo = repo;
        _currencyConverter = currencyConverter;
        _eventProducer = eventProducer;
    }

    public async Task Handle(Transfer command, CancellationToken cancellationToken)
    {
        var transaction = Transaction.Transfer(
            command.SourceAccountId,
            command.DestinationAccountId,
            command.Amount);

        await _repo.PersistAsync(transaction, cancellationToken)
                   .ConfigureAwait(false);

        var @event = new TransactionStarted(Guid.NewGuid(), transaction.Id);
        await _eventProducer.DispatchAsync(@event, cancellationToken)
                            .ConfigureAwait(false);
    }
}
