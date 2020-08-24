using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Core;
using SuperSafeBank.Core.EventBus;
using SuperSafeBank.Core.Models;

namespace SuperSafeBank.Persistence.Azure
{
    public class EventConsumer<TA, TKey> : IEventConsumer<TA, TKey>
        where TA : IAggregateRoot<TKey>
    {
        private readonly ISubscriptionClient _subscriptionClient;
        private readonly IEventSerializer _eventSerializer;
        private readonly ILogger<EventConsumer<TA, TKey>> _logger;

        public EventConsumer(ISubscriptionClientFactory subscriptionClientFactory, string topicBaseName, string subscriptionName, IEventSerializer eventSerializer, ILogger<EventConsumer<TA, TKey>> logger)
        {
            if (subscriptionClientFactory == null) throw new ArgumentNullException(nameof(subscriptionClientFactory));
            if (string.IsNullOrWhiteSpace(topicBaseName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(topicBaseName));
            if (string.IsNullOrWhiteSpace(subscriptionName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(subscriptionName));

            _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var aggregateType = typeof(TA);
            var topicName = $"{topicBaseName}-{aggregateType.Name}";
            _subscriptionClient = subscriptionClientFactory.Build(topicName, subscriptionName);
        }

        public async Task ConsumeAsync(CancellationToken stoppingToken)
        {
            _subscriptionClient.RegisterMessageHandler(
                async (msg, tkn) =>
                {
                    var eventType = msg.UserProperties["type"] as string;
                    var @event = _eventSerializer.Deserialize<TKey>(eventType, msg.Body);
                    if (null == @event)
                        throw new SerializationException($"unable to deserialize event {eventType} : {msg.Body}");

                    await OnEventReceived(@event);

                    await _subscriptionClient.CompleteAsync(msg.SystemProperties.LockToken);
                },
                new MessageHandlerOptions(ex =>
                {
                    _logger.LogError(ex.Exception, $"an error has occurred while processing message: {ex.Exception.Message}");
                    return Task.CompletedTask;
                })
                {
                    AutoComplete = false
                });
        }

        public event EventReceivedHandler<TKey> EventReceived;
        private Task OnEventReceived(IDomainEvent<TKey> e)
        {
            var handler = EventReceived;
            return handler?.Invoke(this, e);
        }
    }
}