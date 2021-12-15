using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Core;
using SuperSafeBank.Core.EventBus;
using SuperSafeBank.Core.Models;
using Azure.Messaging.ServiceBus;

namespace SuperSafeBank.Transport.Azure
{
    public class EventProducer<TA, TKey> : IEventProducer<TA, TKey>, IAsyncDisposable
        where TA : IAggregateRoot<TKey>
    {
        private readonly ILogger<EventProducer<TA, TKey>> _logger;

        private ServiceBusSender _sender;
        
        private readonly IEventSerializer _eventSerializer;

        public EventProducer(ServiceBusClient senderFactory, 
                            string topicBaseName, 
                            IEventSerializer eventSerializer, 
                            ILogger<EventProducer<TA, TKey>> logger)
        {
            if (senderFactory == null) 
                throw new ArgumentNullException(nameof(senderFactory));
            if (string.IsNullOrWhiteSpace(topicBaseName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(topicBaseName));

            _logger = logger;
            _eventSerializer = eventSerializer;
            var aggregateType = typeof(TA);

            this.TopicName = $"{topicBaseName}-{aggregateType.Name}";
            _sender = senderFactory.CreateSender(this.TopicName);
        }
        
        public async Task DispatchAsync(TA aggregateRoot)
        {
            if (null == aggregateRoot)
                throw new ArgumentNullException(nameof(aggregateRoot));

            if (!aggregateRoot.Events.Any())
                return;

            _logger.LogInformation("publishing " + aggregateRoot.Events.Count + " events for {AggregateId} ...", aggregateRoot.Id);

            using ServiceBusMessageBatch messageBatch = await _sender.CreateMessageBatchAsync();

            foreach(var @event in aggregateRoot.Events)
            {
                var eventType = @event.GetType();

                var serialized = _eventSerializer.Serialize(@event);

                var message = new ServiceBusMessage(serialized)
                {
                    CorrelationId = aggregateRoot.Id.ToString(),
                    ApplicationProperties =
                    {
                        {"aggregate", aggregateRoot.Id.ToString()},
                        {"type", eventType.AssemblyQualifiedName}
                    }
                };
                messageBatch.TryAddMessage(message);
            }

            await _sender.SendMessagesAsync(messageBatch);
        }

        public async ValueTask DisposeAsync()
        {
            if(_sender is not null)
                await _sender.DisposeAsync();
            _sender = null;
        }

        public string TopicName { get; }
    }
}
