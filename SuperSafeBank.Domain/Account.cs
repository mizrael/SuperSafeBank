using System;
using SuperSafeBank.Core.Models;
using SuperSafeBank.Domain.Events;
using SuperSafeBank.Domain.Services;

namespace SuperSafeBank.Domain
{
    public class Account : BaseAggregateRoot<Account, Guid>
    {
        private Account() { }

        public Account(Guid id, Customer owner, Currency currency) : base(id)
        {
            if (owner == null) 
                throw new ArgumentNullException(nameof(owner));
            if (currency == null)
                throw new ArgumentNullException(nameof(currency));

            this.OwnerId = owner.Id;
            this.Balance = Money.Zero(currency);
            
            this.Append(new AccountCreated(this));
        }

        public Guid OwnerId { get; private set; }
        public Money Balance { get; private set; }

        public void Withdraw(Money amount, ICurrencyConverter currencyConverter)
        {
            if (amount.Value < 0)
                throw new ArgumentOutOfRangeException(nameof(amount),"amount cannot be negative");

            var normalizedAmount = currencyConverter.Convert(amount, this.Balance.Currency);
            if (normalizedAmount.Value > this.Balance.Value)
                throw new AccountTransactionException($"unable to withdrawn {normalizedAmount} from account {this.Id}", this);

            this.Append(new Withdrawal(this, amount));
        }

        public void Deposit(Money amount, ICurrencyConverter currencyConverter)
        {
            if(amount.Value < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "amount cannot be negative");
            
            var normalizedAmount = currencyConverter.Convert(amount, this.Balance.Currency);
            
            this.Append(new Deposit(this, normalizedAmount));
        }

        protected override void When(IDomainEvent<Guid> @event)
        {
            switch (@event)
            {
                case AccountCreated c:
                    this.Id = c.AggregateId;
                    this.Balance = new Money(c.Currency, 0);
                    this.OwnerId = c.OwnerId;
                    break;
                case Withdrawal w:
                    this.Balance = this.Balance.Subtract(w.Amount.Value);
                    break;
                case Deposit d:
                    this.Balance = this.Balance.Add(d.Amount.Value);
                    break;
            }
        }

        public static Account Create(Customer owner, Currency currency)
        {
            return new Account(Guid.NewGuid(), owner, currency);
        }
    }
}