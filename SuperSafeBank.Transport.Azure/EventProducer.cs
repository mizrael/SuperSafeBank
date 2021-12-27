using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Common.EventBus;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSafeBank.Transport.Azure
{
    public class EventProducer : IEventProducer, IAsyncDisposable        
    {
        private readonly ILogger<EventProducer> _logger;

        private ServiceBusSender _sender;
                
        public EventProducer(ServiceBusClient senderFactory, 
                            string topicName, 
                            ILogger<EventProducer> logger)
        {
            if (senderFactory == null) 
                throw new ArgumentNullException(nameof(senderFactory));
            if (string.IsNullOrWhiteSpace(topicName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(topicName));

            _logger = logger;            
            _sender = senderFactory.CreateSender(topicName);
        }

        public async Task DispatchAsync(IIntegrationEvent @event, CancellationToken cancellationToken = default)            
        {
            if (null == @event)
                throw new ArgumentNullException(nameof(@event));

            _logger.LogInformation("publishing event {EventId} ...", @event.Id);

            var eventType = @event.GetType();

            var serialized = System.Text.Json.JsonSerializer.Serialize(@event, eventType);

            var message = new ServiceBusMessage(serialized)
            {
                MessageId = @event.Id.ToString(),
                ApplicationProperties =
                {
                    {"type", eventType.AssemblyQualifiedName}
                }
            };
            
            await _sender.SendMessageAsync(message);
        }

        public async ValueTask DisposeAsync()
        {
            if(_sender is not null)
                await _sender.DisposeAsync();
            _sender = null;
        }
    }
}
