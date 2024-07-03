using MediatR;
using SuperSafeBank.Common.EventBus;
using System;

namespace SuperSafeBank.Domain.IntegrationEvents;

public record TransactionStarted : IIntegrationEvent, INotification
{
    public TransactionStarted(Guid id, Guid transactionId)
    {
        this.Id = id;
        this.TransactionId = transactionId;
    }

    public Guid TransactionId { get; init; }
    public Guid Id { get; }
}