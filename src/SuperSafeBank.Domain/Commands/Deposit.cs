using MediatR;
using SuperSafeBank.Common;
using SuperSafeBank.Common.EventBus;
using SuperSafeBank.Domain.IntegrationEvents;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSafeBank.Domain.Commands;

public record Deposit : IRequest
{
    public Deposit(Guid accountId, Money amount)
    {
        AccountId = accountId;
        Amount = amount ?? throw new ArgumentNullException(nameof(amount));
    }

    public Guid AccountId { get; }
    public Money Amount { get; }
}

public class DepositHandler : IRequestHandler<Deposit>
{
    private readonly IAggregateRepository<Account, Guid> _accountEventsService;
    private readonly IAggregateRepository<Transaction, Guid> _transactionRepo;
    private readonly IEventProducer _eventProducer;

    public DepositHandler(
        IAggregateRepository<Account, Guid> accountsRepo,
        IAggregateRepository<Transaction, Guid> transactionsRepo, 
        IEventProducer eventProducer)
    {
        _accountEventsService = accountsRepo;
        _transactionRepo = transactionsRepo;
        _eventProducer = eventProducer;
    }

    public async Task Handle(Deposit command, CancellationToken cancellationToken)
    {
        var account = await _accountEventsService.RehydrateAsync(command.AccountId);
        if(null == account)
            throw new ArgumentOutOfRangeException(nameof(Deposit.AccountId), "invalid account id");

        var transaction = Transaction.Deposit(account, command.Amount);
        
        await _transactionRepo.PersistAsync(transaction);

        var @event = new TransactionStarted(Guid.NewGuid(), transaction.Id);
        await _eventProducer.DispatchAsync(@event, cancellationToken)
                            .ConfigureAwait(false);
    }
}
