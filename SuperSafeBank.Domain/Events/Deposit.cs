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
        }

        public Money Amount { get; init; }
    }
}