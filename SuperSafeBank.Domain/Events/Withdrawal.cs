using System;
using SuperSafeBank.Core.Models;

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
            OwnerId = account.OwnerId;
        }

        public Money Amount { get; init; }
        public Guid OwnerId { get; init; }
    }
}