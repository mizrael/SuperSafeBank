using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Core;
using SuperSafeBank.Core.Models;
using SuperSafeBank.Domain;
using SuperSafeBank.Persistence.Kafka;

namespace SuperSafeBank.Web.API.Workers
{
    public class EventsConsumerWorker : BackgroundService
    {
        private readonly IMediator _mediator;
        private readonly ILogger<EventsConsumerWorker> _logger;
        private readonly string _eventsTopicName;
        private readonly string _kafkaConnString;
        private readonly IEventDeserializer _eventDeserializer;

        public EventsConsumerWorker(IMediator mediator, ILogger<EventsConsumerWorker> logger, string eventsTopicName, string kafkaConnString, IEventDeserializer eventDeserializer)
        {
            _mediator = mediator;
            _logger = logger;
            _eventsTopicName = eventsTopicName;
            _kafkaConnString = kafkaConnString;
            _eventDeserializer = eventDeserializer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var tc = Task.WhenAll(new[]
            {
                InitEventConsumer<Account, Guid>(stoppingToken),
                InitEventConsumer<Customer, Guid>(stoppingToken),
            });
            await tc;
        }

        private Task InitEventConsumer<TA, TK>(
             CancellationToken cancellationToken)
            where TA : IAggregateRoot<TK>
        {
            var consumer = new EventConsumer<TA, TK>(_eventsTopicName, _kafkaConnString, _eventDeserializer);

            async Task onEventReceived(object s, IDomainEvent<TK> e)
            {
                var @event = EventReceivedFactory.Create((dynamic)e);

                _logger.LogInformation($"Received event {@event.GetType()} for aggregate {e.AggregateId} , version {e.AggregateVersion}");

                await _mediator.Publish(@event, cancellationToken);
            }
            consumer.EventReceived += onEventReceived;

            consumer.ExceptionThrown += (sender, exception) =>
            {
                _logger.LogError(exception, $"an exception has occurred while consuming a message: {exception.Message}");
            };

            return consumer.ConsumeAsync(cancellationToken);
        }
    }
}
