using System;

namespace SuperSafeBank.Core.Models.Events
{
    public class AccountCreated : BaseDomainEvent<Account, Guid>
    {
        public AccountCreated(Account account) : base(account)
        {
            OwnerId = account.Owner.Id;
            Currency = account.Balance.Currency;
        }

        public Guid OwnerId { get; }
        public Currency Currency { get; }
    }
}