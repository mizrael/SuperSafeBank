using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SuperSafeBank.Common;
using SuperSafeBank.Domain.Services;
using SuperSafeBank.Persistence.EventStore;
using SuperSafeBank.Service.Core.Common;
using SuperSafeBank.Service.Core.Persistence.EventStore;
using SuperSafeBank.Service.Core.Persistence.Mongo;
using SuperSafeBank.Service.Core.Persistence.Mongo.EventHandlers;
using SuperSafeBank.Transport.Kafka;
using System;

namespace SuperSafeBank.Service.Core.Registries
{
    public record Infrastructure(string EventBus, string AggregateStore, string QueryDb);

    public static class InfrastructureRegistry
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            var infraConfig = config.GetSection("infrastructure").Get<Infrastructure>();

            return services.RegisterQueryDb(config, infraConfig)
                    .RegisterEventBus(config, infraConfig)
                    .RegisterAggregateStore(config, infraConfig)
                    .Scan(scan =>
                    {
                        scan.FromAssembliesOf(typeof(CustomerDetailsHandler))
                            .RegisterHandlers(typeof(IRequestHandler<>))
                            .RegisterHandlers(typeof(IRequestHandler<,>))
                            .RegisterHandlers(typeof(INotificationHandler<>));
                    });
        }

        private static IServiceCollection RegisterAggregateStore(this IServiceCollection services, IConfiguration config, Infrastructure infraConfig)
        {
            if (infraConfig.AggregateStore == "EventStore")
            {
                var eventstoreConnStr = config.GetConnectionString("eventstore");
                services.AddEventStore(eventstoreConnStr)
                    .AddSingleton<IAggregateRepository<CustomerEmail, string>, AggregateRepository<CustomerEmail, string>>()
                    .AddTransient<ICustomerEmailsService, Persistence.EventStore.CustomerEmailsService>();
            }
            else throw new ArgumentOutOfRangeException($"invalid aggregate store type: {infraConfig.AggregateStore}");

            return services;
        }

        private static IServiceCollection RegisterEventBus(this IServiceCollection services, IConfiguration config, Infrastructure infraConfig)
        {
            if (infraConfig.EventBus == "Kafka")
            {
                var producerConfig = new KafkaProducerConfig(config.GetConnectionString("kafka"), config["eventsTopicName"]);
                services.AddKafkaTransport(producerConfig);
            }
            else throw new ArgumentOutOfRangeException($"invalid event bus type: {infraConfig.EventBus}");

            return services;
        }

        private static IServiceCollection RegisterQueryDb(this IServiceCollection services, IConfiguration config, Infrastructure infraConfig)
        {
            if (infraConfig.QueryDb == "MongoDb")
            {
                var mongoConnStr = config.GetConnectionString("mongo");
                var mongoQueryDbName = config["queryDbName"];
                var mongoConfig = new MongoConfig(mongoConnStr, mongoQueryDbName);
                services.AddMongoDb(mongoConfig);
            }
            else throw new ArgumentOutOfRangeException($"invalid read db type: {infraConfig.QueryDb}");
            
            return services;
        }
    }
}