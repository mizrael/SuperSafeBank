using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using MediatR;
using SuperSafeBank.Core;
using SuperSafeBank.Core.EventBus;

namespace SuperSafeBank.Web.Persistence.Azure.Functions.Functions
{
    public abstract class BaseFunction
    {
        private readonly IEventSerializer _eventSerializer;
        private readonly IMediator _mediator;

        protected BaseFunction(IEventSerializer eventSerializer, IMediator mediator)
        {
            _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        protected async Task HandleMessage(ServiceBusMessage msg)
        {
            var eventType = msg.ApplicationProperties["type"] as string;

            //TODO: evaluate move to async deserialization to avoid .ToArray() call
            var domainEvent = _eventSerializer.Deserialize<Guid>(eventType, msg.Body.ToArray());
            if (null == domainEvent)
                throw new SerializationException($"unable to deserialize event {eventType} : {msg.Body}");

            var @event = EventReceivedFactory.Create((dynamic)domainEvent);
            await _mediator.Publish(@event, CancellationToken.None);
        }
    }
}