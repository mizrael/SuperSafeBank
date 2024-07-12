using System;
using System.Collections.Generic;
using System.Security.Principal;
using SuperSafeBank.Common.Models;
using SuperSafeBank.Domain.DomainEvents;
using SuperSafeBank.Domain.Services;

namespace SuperSafeBank.Domain;

public record Transaction : BaseAggregateRoot<Transaction, Guid>
{
    private Transaction() { }

    public Transaction(Guid id, string type, string[] states, IDictionary<string, string> properties) : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type, nameof(type));
        ArgumentNullException.ThrowIfNull(states, nameof(states));
        ArgumentNullException.ThrowIfNull(properties, nameof(properties));
        
        this.Append(new TransactionEvents.TransactionCreated(this, type, states, properties));
    }

    protected override void When(IDomainEvent<Guid> @event)
    {
        switch(@event)
        {
            case TransactionEvents.TransactionCreated tc:
                this.Type = tc.Type;
                this.States = tc.States;
                this.Properties = tc.Properties;
                break;
            case TransactionEvents.StepForward sf:

                if (string.IsNullOrWhiteSpace(sf.OldState))
                    CurrentState = States[0];
                else
                {
                    var index = Array.IndexOf(States, sf.OldState);
                    if (index == States.Length - 1)
                        throw new InvalidOperationException("transaction already completed");
                    CurrentState = States[index + 1];
                }
                break;
        }
    }
    public void StepForward()
    {
        this.Append(new TransactionEvents.StepForward(this));
    }

    public IReadOnlyDictionary<string, string> Properties { get; private set; }

    public string[] States { get; private set; }

    public string CurrentState { get; private set; }

    public bool IsCompleted => States.Length > 0 && States[^1] == CurrentState;

    public string Type { get; private set; }

    #region helpers

    public bool TryGetSourceAccountId(out Guid accountId)
    {
        if (Properties.TryGetValue(TransactionProperties.SourceAccount, out var value))
            return Guid.TryParse(value, out accountId);

        accountId = Guid.Empty;
        return false;
    }

    public bool TryGetDestinationAccountId(out Guid accountId)
    {
        if (Properties.TryGetValue(TransactionProperties.DestinationAccount, out var value))
            return Guid.TryParse(value, out accountId);

        accountId = Guid.Empty;
        return false;
    }

    public bool TryGetAmount(out Money? amount)
    {
        if (Properties.TryGetValue(TransactionProperties.Amount, out var value))
            return Money.TryParse(value, out amount);

        amount = null;
        return false;
    }

    #endregion helpers

    #region factories

    public static Transaction Transfer(Account sourceAccount, Account destinationAccount, Money amount)
    => new Transaction(
            Guid.NewGuid(),
            TransactionTypes.Transfer,
            TransactionTypes.TransferStates,
            new Dictionary<string, string>
            {
                { TransactionProperties.SourceAccount, sourceAccount.Id.ToString() },
                { TransactionProperties.DestinationAccount, destinationAccount.Id.ToString() },
                { TransactionProperties.Amount, amount.ToString() }
            });

    public static Transaction Deposit(Account account, Money amount)
    => new Transaction(
            Guid.NewGuid(),
            TransactionTypes.Deposit,
            TransactionTypes.DepositStates,
            new Dictionary<string, string>
            {
                { TransactionProperties.DestinationAccount, account.Id.ToString() },
                { TransactionProperties.Amount, amount.ToString() }
            });

    public static Transaction Withdraw(Account account, Money amount, ICurrencyConverter currencyConverter)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(amount.Value, 0, nameof(amount));

        var normalizedAmount = currencyConverter.Convert(amount, account.Balance.Currency);
        if (normalizedAmount.Value > account.Balance.Value)
            throw new AccountTransactionException($"unable to withdrawn {normalizedAmount} from account {account.Id}", account);

        return new Transaction(
            Guid.NewGuid(),
            TransactionTypes.Withdraw,
            TransactionTypes.WithdrawStates,
            new Dictionary<string, string>
            {
                { TransactionProperties.SourceAccount, account.Id.ToString() },
                { TransactionProperties.Amount, amount.ToString() }
            });
    }

    #endregion factories
}
