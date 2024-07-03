using System;
using System.Collections.Generic;
using SuperSafeBank.Common.Models;
using SuperSafeBank.Domain.DomainEvents;
using SuperSafeBank.Domain.Services;

namespace SuperSafeBank.Domain
{
    public record Account : BaseAggregateRoot<Account, Guid>
    {
        private readonly HashSet<Guid> _processedTransactions = new();

        private Account() { }

        public Account(Guid id, Customer owner, Currency currency) : base(id)
        {
            if (owner == null) 
                throw new ArgumentNullException(nameof(owner));
            if (currency == null)
                throw new ArgumentNullException(nameof(currency));
                        
            this.Append(new AccountEvents.AccountCreated(this, owner, currency));
        }

        public Guid OwnerId { get; private set; }
        public Money Balance { get; private set; }

        public void Withdraw(Transaction transaction, ICurrencyConverter currencyConverter)
        {
            ArgumentNullException.ThrowIfNull(transaction, nameof(transaction));
            ArgumentNullException.ThrowIfNull(currencyConverter, nameof(currencyConverter));

            ArgumentOutOfRangeException.ThrowIfNotEqual(transaction.Type, TransactionTypes.Deposit, nameof(transaction.Type));

            if (!transaction.TryGetSourceAccountId(out var sourceAccountId) || sourceAccountId != this.Id)
                throw new ArgumentException("invalid source account id", nameof(transaction));

            if (!transaction.TryGetAmount(out var amount))
                throw new ArgumentException("invalid amount", nameof(transaction));

            ArgumentOutOfRangeException.ThrowIfLessThan(amount.Value, 0, nameof(amount));

            if (_processedTransactions.Contains(transaction.Id))
                return;

            var normalizedAmount = currencyConverter.Convert(amount, this.Balance.Currency);
            if (normalizedAmount.Value > this.Balance.Value)
                throw new AccountTransactionException($"unable to withdrawn {normalizedAmount} from account {this.Id}", this);

            this.Append(new AccountEvents.Withdrawal(this, amount, transaction));
        }

        public void Deposit(Transaction transaction, ICurrencyConverter currencyConverter)
        {
            ArgumentNullException.ThrowIfNull(transaction, nameof(transaction));
            ArgumentNullException.ThrowIfNull(currencyConverter, nameof(currencyConverter)); 
            
            ArgumentOutOfRangeException.ThrowIfNotEqual(transaction.Type, TransactionTypes.Deposit, nameof(transaction.Type));
            
            if(!transaction.TryGetDestinationAccountId(out var destinationAccountId) || destinationAccountId != this.Id)
                throw new ArgumentException("invalid destination account id", nameof(transaction)); 
            
            if(!transaction.TryGetAmount(out var amount))
                throw new ArgumentException("invalid amount", nameof(transaction));

            ArgumentOutOfRangeException.ThrowIfLessThan(amount.Value, 0, nameof(amount));            

            if (_processedTransactions.Contains(transaction.Id))
                return;

            var normalizedAmount = currencyConverter.Convert(amount, this.Balance.Currency);
            
            this.Append(new AccountEvents.Deposit(this, normalizedAmount, transaction));
        }

        protected override void When(IDomainEvent<Guid> @event)
        {
            switch (@event)
            {
                case AccountEvents.AccountCreated c:
                    this.Id = c.AggregateId;
                    this.Balance = Money.Zero(c.Currency);
                    this.OwnerId = c.OwnerId;
                    break;
                case AccountEvents.Withdrawal w:
                    this.Balance = this.Balance.Subtract(w.Amount);
                    _processedTransactions.Add(w.TransactionId);
                    break;
                case AccountEvents.Deposit d:
                    this.Balance = this.Balance.Add(d.Amount);
                    _processedTransactions.Add(d.TransactionId);
                    break;
            }
        }

        public static Account Create(Guid accountId, Customer owner, Currency currency)
        {
            var account = new Account(accountId, owner, currency);
            owner.AddAccount(account);
            return account;
        }
    }
}