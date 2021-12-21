using System;
using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Domain.Events
{
    public record AccountCreated : BaseDomainEvent<Account, Guid>
    {
        /// <summary>
        /// for deserialization
        /// </summary>
        private AccountCreated() { }

        public AccountCreated(Account account, Customer owner, Currency currency) : base(account)
        {
            if (owner is null)
                throw new ArgumentNullException(nameof(owner));
            
            OwnerId = owner.Id;
            Currency = currency ?? throw new ArgumentNullException(nameof(currency));
        }

        public Guid OwnerId { get; init; }
        public Currency Currency { get; init; }
    }
}