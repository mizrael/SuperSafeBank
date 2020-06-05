using System;
using SuperSafeBank.Core.Models;

namespace SuperSafeBank.Domain.Events
{
    public class Deposit : BaseDomainEvent<Account, Guid>
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

        public Money Amount { get; private set; }
        public Guid OwnerId { get; private set; }
    }
}