using MediatR;
using SuperSafeBank.Common.EventBus;
using System;

namespace SuperSafeBank.Domain.IntegrationEvents
{
    public record CustomerCreated : IIntegrationEvent, INotification
    {
        public CustomerCreated(Guid id, Guid customerId)
        {
            this.Id = id;
            this.CustomerId = customerId;
        }

        public Guid Id { get; }
        public Guid CustomerId { get; }
    }

    public record AccountCreated : IIntegrationEvent, INotification
    {
        public AccountCreated(Guid id, Guid accountId)
        {
            this.Id = id;
            this.AccountId = accountId;
        }

        public Guid AccountId { get; init; }
        public Guid Id { get; }
    }

    public record TransactionHappened : IIntegrationEvent, INotification
    {
        public TransactionHappened(Guid id, Guid accountId)
        {
            this.Id = id;
            this.AccountId = accountId;
        }

        public Guid AccountId { get; init; }
        public Guid Id { get; }

    }
}