using System;

namespace SuperSafeBank.Core.Models.Events
{
    public class Withdrawal : BaseDomainEvent<Guid>
    {
        private Withdrawal(Money amount, Guid accountId, DateTime when) : base(accountId, when) 
        {
            Amount = amount;
        }

        public Money Amount { get; }

        public static Withdrawal Create(Account account, Money amount)
        {
            return new Withdrawal(amount, account.Id, DateTime.UtcNow);
        }
    }
}