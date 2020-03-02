using System;
using SuperSafeBank.Core.Models.Events;
using SuperSafeBank.Core.Services;

namespace SuperSafeBank.Core.Models
{
    public class Account : BaseAggregateRoot<Account, Guid>
    {
        public Account(Guid id, Customer owner, Currency currency) : base(id)
        {
            this.Owner = owner;
            this.Balance = Money.Zero(currency);

            this.AddEvent(new AccountCreated(this));
        }

        public Customer Owner { get; }
        public Money Balance { get; private set; }

        public void Withdraw(Money amount, ICurrencyConverter currencyConverter)
        {
            if (amount.Value < 0)
                throw new ArgumentOutOfRangeException(nameof(amount),"amount cannot be negative");

            var normalizedAmount = currencyConverter.Convert(amount, this.Balance.Currency);
            if (normalizedAmount.Value > this.Balance.Value)
                throw new AccountTransactionException($"unable to withdrawn {normalizedAmount} from account {this.Id}", this);

            this.AddEvent(new Withdrawal(this, amount));
        }

        public void Deposit(Money amount, ICurrencyConverter currencyConverter)
        {
            if(amount.Value < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "amount cannot be negative");
            
            var normalizedAmount = currencyConverter.Convert(amount, this.Balance.Currency);
            
            this.AddEvent(new Models.Events.Deposit(this, normalizedAmount));
        }

        protected override void Apply(IDomainEvent<Guid> @event)
        {
            switch (@event)
            {
                case Models.Events.Withdrawal w:
                    this.Balance = this.Balance.Subtract(w.Amount.Value);
                    break;
                case Models.Events.Deposit d:
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