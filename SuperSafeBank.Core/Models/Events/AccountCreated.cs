using System;

namespace SuperSafeBank.Core.Models.Events
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
            OwnerId = account.Owner.Id;
            Currency = account.Balance.Currency;
        }

        public Guid OwnerId { get; private set; }
        public Currency Currency { get; private set; }
    }
}