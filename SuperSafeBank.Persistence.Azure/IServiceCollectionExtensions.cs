using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Core;
using SuperSafeBank.Core.EventBus;
using SuperSafeBank.Core.Models;
using SuperSafeBank.Domain;

namespace SuperSafeBank.Persistence.Azure
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            return services.AddSingleton<CosmosClient>(ctx =>
            {
                var connectionString = config.GetConnectionString("cosmos");
                return new CosmosClient(connectionString);
            }).AddSingleton<ITopicClientFactory>(ctx =>
            {
                var connectionString = config.GetConnectionString("producer");
                return new TopicClientFactory(connectionString);
            }).AddEventsProducer<Customer, Guid>(config)
              .AddEventsProducer<Account, Guid>(config)
              .AddEventsRepository<Customer, Guid>(config)
              .AddEventsRepository<Account, Guid>(config);
        }

        private static IServiceCollection AddEventsRepository<TA, TK>(this IServiceCollection services, IConfiguration config)
            where TA : class, IAggregateRoot<TK>
        {
            var dbName = config["dbName"];
            return services.AddSingleton<IEventsRepository<TA, TK>>(ctx =>
            {
                var connectionWrapper = ctx.GetRequiredService<CosmosClient>();
                var eventDeserializer = ctx.GetRequiredService<IEventSerializer>();
                return new EventsRepository<TA, TK>(connectionWrapper, dbName, eventDeserializer);
            });
        }

        private static IServiceCollection AddEventsProducer<TA, TK>(this IServiceCollection services, IConfiguration config)
            where TA : class, IAggregateRoot<TK>
        {
            var topicsBaseName = config["topicsBaseName"];
            return services.AddSingleton<IEventProducer<TA, TK>>(ctx =>
            {
                var clientFactory = ctx.GetRequiredService<ITopicClientFactory>();
                var eventDeserializer = ctx.GetRequiredService<IEventSerializer>();
                var logger = ctx.GetRequiredService<ILogger<EventProducer<TA, TK>>>();
                return new EventProducer<TA, TK>(clientFactory, topicsBaseName, eventDeserializer, logger);
            });
        }
    }
}