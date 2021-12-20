using System;
using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Domain.Events
{
    public record Deposit : BaseDomainEvent<Account, Guid>
    {
        /// <summary>
        /// for deserialization
        /// </summary>
        private Deposit() { }

        public Deposit(Account account, Money amount) : base(account)
        {
            Amount = amount;
            OwnerId = account.OwnerId;
        }

        public Money Amount { get; init; }
        public Guid OwnerId { get; init; }
    }
}