using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using SuperSafeBank.Common;
using SuperSafeBank.Common.EventBus;
using SuperSafeBank.Common.Models;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.Services;
using SuperSafeBank.Persistence.EventStore;
using SuperSafeBank.Persistence.Mongo;
using SuperSafeBank.Service.Core.Common;
using SuperSafeBank.Service.Core.Persistence.Mongo;
using SuperSafeBank.Service.Core.Persistence.Mongo.EventHandlers;
using SuperSafeBank.Transport.Kafka;
using System;

namespace SuperSafeBank.Service.Core.Registries
{
    public static class InfrastructureRegistry
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            var eventstoreConnStr = config.GetConnectionString("eventstore");
            var producerConfig = new EventsProducerConfig(config.GetConnectionString("kafka"), config["eventsTopicName"]);

            var mongoConnStr = config.GetConnectionString("mongo");
            var mongoQueryDbName = config["queryDbName"];
            var mongoConfig = new MongoConfig(mongoConnStr, mongoQueryDbName);

            return services.Scan(scan =>
            {
                scan.FromAssembliesOf(typeof(CustomerDetailsHandler))
                    .RegisterHandlers(typeof(IRequestHandler<>))
                    .RegisterHandlers(typeof(IRequestHandler<,>))
                    .RegisterHandlers(typeof(INotificationHandler<>));
            }).AddMongoDb(mongoConfig)
            .AddKafkaEventProducer<Customer, Guid>(producerConfig)
            .AddKafkaEventProducer<Account, Guid>(producerConfig)
            .AddEventStore(eventstoreConnStr)
            .AddSingleton<ICustomerEmailsService>(ctx =>
            {
                var dbName = config["commandsDbName"];
                var client = ctx.GetRequiredService<MongoClient>();
                var database = client.GetDatabase(dbName);
                return new CustomerEmailsService(database);
            });
        }
    }
}