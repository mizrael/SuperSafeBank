using MediatR;
using SuperSafeBank.Common.EventBus;
using System;

namespace SuperSafeBank.Domain.IntegrationEvents;

public record AccountUpdated : IIntegrationEvent, INotification
{
    public AccountUpdated(Guid id, Guid accountId)
    {
        this.Id = id;
        this.AccountId = accountId;
    }

    public Guid AccountId { get; init; }
    public Guid Id { get; }
}
