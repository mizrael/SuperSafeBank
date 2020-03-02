using System;

namespace SuperSafeBank.Core.Models.Events
{
    public class Deposit : BaseDomainEvent<Account, Guid>
    {
        public Deposit(Account account, Money amount) : base(account)
        {
            Amount = amount;
        }

        public Money Amount { get; }
    }
}