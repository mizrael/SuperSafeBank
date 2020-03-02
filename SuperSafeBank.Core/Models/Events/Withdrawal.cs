using System;

namespace SuperSafeBank.Core.Models.Events
{
    public class Withdrawal : BaseDomainEvent<Account, Guid>
    {
        public Withdrawal(Account account, Money amount) : base(account)
        {
            Amount = amount;
        }

        public Money Amount { get; }
    }
}