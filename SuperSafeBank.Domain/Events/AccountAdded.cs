using System;
using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Domain.Events
{
    public record AccountAdded : BaseDomainEvent<Customer, Guid>
    {
        private AccountAdded() { }        

        public AccountAdded(Customer customer, Guid accountId) : base(customer)
        {
            AccountId = accountId;
        }

        public Guid AccountId { get; init; }
    }
}