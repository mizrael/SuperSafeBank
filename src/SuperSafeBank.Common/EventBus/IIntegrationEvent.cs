using System;

namespace SuperSafeBank.Common.EventBus
{
    public interface IIntegrationEvent
    {
        Guid Id { get; }
    }
}