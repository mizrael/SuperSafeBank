using System;

namespace SuperSafeBank.Core.Models.Events
{
    public class Deposit : BaseDomainEvent<Guid>
    {
        private Deposit(Money amount, Guid accountId, DateTime when) : base(accountId, when)
        {
            Amount = amount;
        }

        public Money Amount { get; }

        public static Deposit Create(Account account, Money amount)
        {
            return new Deposit(amount, account.Id, DateTime.UtcNow);
        }
    }
}