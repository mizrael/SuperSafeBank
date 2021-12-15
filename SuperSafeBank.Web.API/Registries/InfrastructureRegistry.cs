using System;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SuperSafeBank.Core;
using SuperSafeBank.Core.EventBus;
using SuperSafeBank.Core.Models;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.Services;

#if OnPremise
using SuperSafeBank.Persistence.EventStore;
using SuperSafeBank.Transport.Kafka;
using SuperSafeBank.Persistence.Mongo;
using SuperSafeBank.Web.Persistence.Mongo;
using SuperSafeBank.Web.Persistence.Mongo.EventHandlers;
using MongoDB.Driver;
#endif

#if OnAzure
using SuperSafeBank.Persistence.Azure;
using SuperSafeBank.Transport.Azure;
using SuperSafeBank.Web.Persistence.Azure.QueryHandlers;
using SuperSafeBank.Web.Persistence.Azure.Services;
#endif

namespace SuperSafeBank.Web.API.Registries
{
    public static class InfrastructureRegistry
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {

#if OnPremise
            services.AddOnPremise(config);
#endif

#if OnAzure
            services.AddAzure(config);
#endif
            return services                
                .AddEventsService<Customer, Guid>()
                .AddEventsService<Account, Guid>();
        }

#if OnPremise

        private static IServiceCollection AddOnPremise(this IServiceCollection services, IConfiguration config)
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

            var eventstoreConnStr = config.GetConnectionString("eventstore");

            return services.AddKafka(consumerConfig)
                .AddEventStore(eventstoreConnStr)
                .AddSingleton<ICustomerEmailsService>(ctx=>
                {
                    var dbName = config["commandsDbName"];
                    var client = ctx.GetRequiredService<MongoClient>();
                    var database = client.GetDatabase(dbName);
                    return new CustomerEmailsService(database);
                });
        }

#endif
#if OnAzure
        
        private static IServiceCollection AddAzure(this IServiceCollection services,IConfiguration config)
            =>services.AddAzurePersistence(config)
                    .AddAzureTransport(config)
                    .Scan(scan =>
                    {
                        scan.FromAssembliesOf(typeof(CustomersArchiveHandler))
                            .RegisterHandlers(typeof(IRequestHandler<>))
                            .RegisterHandlers(typeof(IRequestHandler<,>))
                            .RegisterHandlers(typeof(INotificationHandler<>));
                    })
                .AddSingleton<ICustomerEmailsService, CustomerEmailsService>();

#endif

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