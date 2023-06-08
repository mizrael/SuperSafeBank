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
}