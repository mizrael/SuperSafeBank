using MediatR;
using SuperSafeBank.Common.EventBus;
using System;

namespace SuperSafeBank.Domain.IntegrationEvents
{
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
}