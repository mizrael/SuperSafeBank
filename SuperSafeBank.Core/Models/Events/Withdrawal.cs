using System;

namespace SuperSafeBank.Core.Models.Events
{
    public class Withdrawal : BaseDomainEvent<Account, Guid>
    {
        /// <summary>
        /// for deserialization
        /// </summary>
        private Withdrawal() { }

        public Withdrawal(Account account, Money amount) : base(account)
        {
            Amount = amount;
        }

        public Money Amount { get; private set; }
    }
}