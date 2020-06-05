using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Core;
using SuperSafeBank.Core.EventBus;
using SuperSafeBank.Core.Models;
using SuperSafeBank.Domain;
using SuperSafeBank.Persistence.Kafka;
using SuperSafeBank.Web.API.Workers;

namespace SuperSafeBank.Web.API.Registries
{
    public static class EventConsumerRegistry
    {
        public static IServiceCollection RegisterWorker(this IServiceCollection services)
        {
            services.AddHostedService(ctx =>
            {
                var logger = ctx.GetRequiredService<ILogger<IEventConsumer>>();
                var eventsDeserializer = ctx.GetRequiredService<IEventDeserializer>();
                var scopeFactory = ctx.GetRequiredService<IServiceScopeFactory>();

                var config = ctx.GetRequiredService<IConfiguration>();

                var kafkaConnStr = config.GetConnectionString("kafka");
                var eventsTopicName = config["eventsTopicName"];
                var groupName = config["eventsTopicGroupName"];
                var consumerConfig = new EventConsumerConfig(kafkaConnStr, eventsTopicName, groupName);

                var consumers = new[]
                {
                    BuildEventConsumer<Account, Guid>(consumerConfig, eventsDeserializer, scopeFactory, logger),
                    BuildEventConsumer<Customer, Guid>(consumerConfig, eventsDeserializer, scopeFactory, logger)
                };

                return new EventsConsumerWorker(consumers);
            });

            return services;
        }

        private static IEventConsumer BuildEventConsumer<TA, TK>(EventConsumerConfig consumerConfig, 
            IEventDeserializer eventDeserializer,
            IServiceScopeFactory scopeFactory,
            ILogger<IEventConsumer> logger)
            where TA : IAggregateRoot<TK>
        {
            var consumer = new EventConsumer<TA, TK>(eventDeserializer, consumerConfig);

            async Task onEventReceived(object s, IDomainEvent<TK> e)
            {
                var @event = EventReceivedFactory.Create((dynamic)e);

                logger.LogInformation($"Received event {@event.GetType()} for aggregate {e.AggregateId} , version {e.AggregateVersion}");
               
                using var scope = scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Publish(@event, CancellationToken.None);
            }
            consumer.EventReceived += onEventReceived;

            consumer.ExceptionThrown += (sender, exception) =>
            {
                logger.LogError(exception, $"an exception has occurred while consuming a message: {exception.Message}");
            };

            return consumer;
        }
    }
}