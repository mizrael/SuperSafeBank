using System;
using MediatR;
using SuperSafeBank.Domain;

namespace SuperSafeBank.Service.Core.Common.Queries
{
    public record AccountDetails
    {
        public AccountDetails(Guid id, Guid ownerId, string ownerFirstName, string ownerLastName, string ownerEmail, Money balance)
        {
            Id = id;
            OwnerId = ownerId;
            OwnerFirstName = ownerFirstName;
            OwnerLastName = ownerLastName;
            OwnerEmail = ownerEmail;
            Balance = balance;
        }

        public Guid Id { get; }
        public Guid OwnerId { get; }
        public string OwnerFirstName { get; }
        public string OwnerLastName { get; }
        public string OwnerEmail { get; }
        public Money Balance { get; }
    }

    public record AccountById(Guid AccountId) : IRequest<AccountDetails>;
}