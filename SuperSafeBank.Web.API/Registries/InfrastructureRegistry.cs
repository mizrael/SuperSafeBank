using System;
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
using SuperSafeBank.Web.API.Infrastructure;

namespace SuperSafeBank.Web.API.Registries
{
    public static class InfrastructureRegistry
    {
        public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
        {
            BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;

            if (null == BsonSerializer.SerializerRegistry.GetSerializer<decimal>())
                BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));

            return services.AddSingleton(ctx =>
                {
                    var connStr = configuration.GetConnectionString("mongo");
                    return new MongoClient(connectionString: connStr);
                })
                .AddSingleton(ctx =>
                {
                    var dbName = configuration["queryDbName"];
                    var client = ctx.GetRequiredService<MongoClient>();
                    var database = client.GetDatabase(dbName);
                    return database;
                }).AddSingleton<IQueryDbContext, QueryDbContext>();
        }

        public static IServiceCollection AddEventStore(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddSingleton<IEventStoreConnectionWrapper>(ctx =>
                {
                    var connStr = configuration.GetConnectionString("eventstore");
                    var logger = ctx.GetRequiredService<ILogger<EventStoreConnectionWrapper>>();
                    return new EventStoreConnectionWrapper(new Uri(connStr), logger);
                }).AddEventsRepository<Customer, Guid>()
                .AddEventProducer<Customer, Guid>(configuration)
                .AddEventsService<Customer, Guid>()
                .AddEventsRepository<Account, Guid>()
                .AddEventProducer<Account, Guid>(configuration)
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
                return new EventProducer<TA, TK>(eventsTopicName, connStr);
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