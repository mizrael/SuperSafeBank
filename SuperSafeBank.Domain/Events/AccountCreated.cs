using System;
using SuperSafeBank.Core.Models;

namespace SuperSafeBank.Domain.Events
{
    public class AccountCreated : BaseDomainEvent<Account, Guid>
    {
        /// <summary>
        /// for deserialization
        /// </summary>
       // [Newtonsoft.Json.JsonConstructor] 
        private AccountCreated() { }

        public AccountCreated(Account account) : base(account)
        {
            OwnerId = account.OwnerId;
            Currency = account.Balance.Currency;
        }

        public Guid OwnerId { get; private set; }
        public Currency Currency { get; private set; }
    }
}