using System;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using SuperSafeBank.Core;
using SuperSafeBank.Core.EventBus;
using SuperSafeBank.Core.Models;
using SuperSafeBank.Domain;
using SuperSafeBank.Persistence.EventStore;
using SuperSafeBank.Persistence.Kafka;
using SuperSafeBank.Web.Persistence.Mongo;
using SuperSafeBank.Web.Persistence.Mongo.EventHandlers;

namespace SuperSafeBank.Web.API.Registries
{
    public static class InfrastructureRegistry
    {
        public static IServiceCollection AddEventStore(this IServiceCollection services, IConfiguration config)
        {
            services.Scan(scan =>
            {
                scan.FromAssembliesOf(typeof(CustomerDetailsHandler))
                    .RegisterHandlers(typeof(IRequestHandler<>))
                    .RegisterHandlers(typeof(IRequestHandler<,>))
                    .RegisterHandlers(typeof(INotificationHandler<>));
            }).AddMongoDb(config);

            var kafkaConnStr = config.GetConnectionString("kafka");
            var eventsTopicName = config["eventsTopicName"];
            var groupName = config["eventsTopicGroupName"];
            var consumerConfig = new EventConsumerConfig(kafkaConnStr, eventsTopicName, groupName);

            return services.RegisterKafkaConsumer(consumerConfig)
                .AddSingleton<IEventStoreConnectionWrapper>(ctx =>
                {
                    var connStr = config.GetConnectionString("eventstore");
                    var logger = ctx.GetRequiredService<ILogger<EventStoreConnectionWrapper>>();
                    return new EventStoreConnectionWrapper(new Uri(connStr), logger);
                }).AddEventsRepository<Customer, Guid>()
                .AddEventProducer<Customer, Guid>(config)
                .AddEventsService<Customer, Guid>()
                .AddEventsRepository<Account, Guid>()
                .AddEventProducer<Account, Guid>(config)
                .AddEventsService<Account, Guid>();
        }

        private static IServiceCollection AddEventsRepository<TA, TK>(this IServiceCollection services)
            where TA : class, IAggregateRoot<TK>
        {
            return services.AddSingleton<IEventsRepository<TA, TK>>(ctx =>
            {
                var connectionWrapper = ctx.GetRequiredService<IEventStoreConnectionWrapper>();
                var eventDeserializer = ctx.GetRequiredService<IEventDeserializer>();
                return new EventsRepository<TA, TK>(connectionWrapper, eventDeserializer);
            });
        }
        
        private static IServiceCollection AddEventProducer<TA, TK>(this IServiceCollection services, IConfiguration configuration)
            where TA : class, IAggregateRoot<TK>
        {
            return services.AddSingleton<IEventProducer<TA, TK>>(ctx =>
            {
                var connStr = configuration.GetConnectionString("kafka");
                var eventsTopicName = configuration["eventsTopicName"];
                var logger = ctx.GetRequiredService<ILogger<EventProducer<TA, TK>>>();
                return new EventProducer<TA, TK>(eventsTopicName, connStr, logger);
            });
        }

        private static IServiceCollection AddEventsService<TA, TK>(this IServiceCollection services)
            where TA : class, IAggregateRoot<TK>
        {
            return services.AddSingleton<IEventsService<TA, TK>>(ctx =>
            {
                var eventsProducer = ctx.GetRequiredService<IEventProducer<TA, TK>>();
                var eventsRepo = ctx.GetRequiredService<IEventsRepository<TA, TK>>();

                return new EventsService<TA, TK>(eventsRepo, eventsProducer);
            });
        }
    }
}