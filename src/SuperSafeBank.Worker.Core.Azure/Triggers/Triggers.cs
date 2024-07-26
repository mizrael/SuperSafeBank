using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using SuperSafeBank.Common.EventBus;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace SuperSafeBank.Worker.Core.Azure.Triggers
{
    public class Triggers(IMediator mediator)
    {
        private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        [Function("CustomerTriggers")]
        public async Task Run([ServiceBusTrigger("bank-events", "worker", Connection = "EventsBus")] string message, IDictionary<string, object> userProperties, string messageId)
        {            
            if (!userProperties.TryGetValue("type", out var typeHeader) || typeHeader is null)
                throw new ArgumentException($"unable to reconstruct integration event from message {messageId}");

            var eventTypeName = typeHeader.ToString();
            var eventType = Type.GetType(eventTypeName);
            var @event = JsonSerializer.Deserialize(message, eventType) as IIntegrationEvent;

            await _mediator.Publish(@event);
        }
    }
}
