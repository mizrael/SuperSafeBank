using System;
using SuperSafeBank.Core.Models;

namespace SuperSafeBank.Domain.Events
{
    public record AccountCreated : BaseDomainEvent<Account, Guid>
    {
        /// <summary>
        /// for deserialization
        /// </summary>
        private AccountCreated() { }

        public AccountCreated(Account account) : base(account)
        {
            OwnerId = account.OwnerId;
            Currency = account.Balance.Currency;
        }

        public Guid OwnerId { get; init; }
        public Currency Currency { get; init; }
    }
}