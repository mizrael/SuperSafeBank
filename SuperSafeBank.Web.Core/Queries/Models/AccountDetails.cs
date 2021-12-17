using System;
using SuperSafeBank.Domain;

namespace SuperSafeBank.Web.Core.Queries.Models
{
    public record AccountDetails
    {
        public AccountDetails(Guid id, Guid ownerId, string ownerFirstName, string ownerLastName, Money balance)
        {
            Id = id;
            OwnerId = ownerId;
            OwnerFirstName = ownerFirstName;
            OwnerLastName = ownerLastName;
            Balance = balance;
        }

        public Guid Id { get; }
        public Guid OwnerId { get; }
        public string OwnerFirstName { get; }
        public string OwnerLastName { get; }
        public Money Balance { get; }
    }
}