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
using SuperSafeBank.Persistence.SQLServer;
using SuperSafeBank.Service.Core.Persistence.SQLServer;
using Microsoft.EntityFrameworkCore;

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
                services.AddEventStorePersistence(eventstoreConnStr)
                    .AddSingleton<IAggregateRepository<Persistence.EventStore.CustomerEmail, string>, EventStoreAggregateRepository<Persistence.EventStore.CustomerEmail, string>>()
                    .AddTransient<ICustomerEmailsService, EventStoreCustomerEmailsService>();
            }else if (infraConfig.AggregateStore == "SQLServer")
            {
                var sqlConnString = config.GetConnectionString("sql");
                services.AddSQLServerPersistence(sqlConnString)
                    .AddDbContextPool<CustomerDbContext>(builder =>
                    {
                        builder.UseSqlServer(sqlConnString, opts =>
                        {
                            opts.EnableRetryOnFailure();
                        });
                    }).AddTransient<ICustomerEmailsService, SQLCustomerEmailsService>();
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