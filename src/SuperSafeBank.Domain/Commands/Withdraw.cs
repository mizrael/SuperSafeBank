﻿using MediatR;
using SuperSafeBank.Common;
using SuperSafeBank.Common.EventBus;
using SuperSafeBank.Domain.IntegrationEvents;
using SuperSafeBank.Domain.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSafeBank.Domain.Commands;

public record Withdraw : IRequest
{
    public Withdraw(Guid accountId, Money amount)
    {
        AccountId = accountId;
        Amount = amount ?? throw new ArgumentNullException(nameof(amount));
    }

    public Guid AccountId { get; }
    public Money Amount { get; }
}


public class WithdrawHandler : IRequestHandler<Withdraw>
{
    private readonly IAggregateRepository<Account, Guid> _accountEventsService;
    private readonly IAggregateRepository<Transaction, Guid> _transactionRepo;
    private readonly IEventProducer _eventProducer;
    private readonly ICurrencyConverter _currencyConverter;

    public WithdrawHandler(
        IAggregateRepository<Account, Guid> accountsRepo,
        IAggregateRepository<Transaction, Guid> transactionsRepo,
        IEventProducer eventProducer,
        ICurrencyConverter currencyConverter)
    {
        _accountEventsService = accountsRepo;
        _transactionRepo = transactionsRepo;
        _eventProducer = eventProducer;
        _currencyConverter = currencyConverter;
    }

    public async Task Handle(Withdraw command, CancellationToken cancellationToken)
    {
        var account = await _accountEventsService.RehydrateAsync(command.AccountId);
        if (null == account)
            throw new ArgumentOutOfRangeException(nameof(Withdraw.AccountId), "invalid account id");

        var transaction = Transaction.Withdraw(account, command.Amount, _currencyConverter);

        await _transactionRepo.PersistAsync(transaction);

        var @event = new TransactionStarted(Guid.NewGuid(), transaction.Id);
        await _eventProducer.DispatchAsync(@event, cancellationToken)
                            .ConfigureAwait(false);
    }
}