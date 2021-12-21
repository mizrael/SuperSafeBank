using System;
using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Domain.Events
{
    public record Withdrawal : BaseDomainEvent<Account, Guid>
    {
        /// <summary>
        /// for deserialization
        /// </summary>
        private Withdrawal() { }

        public Withdrawal(Account account, Money amount) : base(account)
        {
            Amount = amount; 
        }

        public Money Amount { get; init; }
    }
}