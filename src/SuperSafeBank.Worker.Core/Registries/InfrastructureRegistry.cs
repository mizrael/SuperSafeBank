using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SuperSafeBank.Common.EventBus;
using SuperSafeBank.Persistence.EventStore;
using SuperSafeBank.Persistence.SQLServer;
using SuperSafeBank.Service.Core.Persistence.Mongo;
using SuperSafeBank.Transport.Kafka;

namespace SuperSafeBank.Worker.Core.Registries
{
    public static class InfrastructureRegistry
    {
        public static IServiceCollection RegisterInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var infraConfig = configuration.GetSection("infrastructure").Get<InfrastructureConfig>();

            var kafkaConnStr = configuration.GetConnectionString("kafka");
            var eventsTopicName = configuration["eventsTopicName"];
            var groupName = configuration["eventsTopicGroupName"];
            var consumerConfig = new EventsConsumerConfig(kafkaConnStr, eventsTopicName, groupName);

            var mongoConnStr = configuration.GetConnectionString("mongo");
            var mongoQueryDbName = configuration["queryDbName"];
            var mongoConfig = new MongoConfig(mongoConnStr, mongoQueryDbName);

            return services.AddSingleton(consumerConfig)
                .AddSingleton(typeof(IEventConsumer), typeof(KafkaEventConsumer))
                .AddMongoDb(mongoConfig)
                .RegisterAggregateStore(configuration, infraConfig);
        }

        private static IServiceCollection RegisterAggregateStore(this IServiceCollection services, IConfiguration config, InfrastructureConfig infraConfig)
        {
            if (infraConfig.AggregateStore == "EventStore")
            {
                var eventstoreConnStr = config.GetConnectionString("eventstore");
                services.AddEventStorePersistence(eventstoreConnStr);
            }
            else if (infraConfig.AggregateStore == "SQLServer")
            {
                var sqlConnString = config.GetConnectionString("sql");
                services.AddSQLServerPersistence(sqlConnString);
            }
            else throw new ArgumentOutOfRangeException($"invalid aggregate store type: {infraConfig.AggregateStore}");

            return services;
        }
    }
}