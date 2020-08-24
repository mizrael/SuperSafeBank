using System;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SuperSafeBank.Core;
using SuperSafeBank.Core.EventBus;
using SuperSafeBank.Core.Models;
using SuperSafeBank.Domain;

#if OnPremise
using SuperSafeBank.Persistence.EventStore;
using SuperSafeBank.Persistence.Kafka;
using SuperSafeBank.Web.Persistence.Mongo;
using SuperSafeBank.Web.Persistence.Mongo.EventHandlers;
#endif

#if OnAzure
using SuperSafeBank.Persistence.Azure;
using SuperSafeBank.Web.Persistence.Azure.QueryHandlers;
#endif

namespace SuperSafeBank.Web.API.Registries
{
    public static class InfrastructureRegistry
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
            IConfiguration config)
        {

#if OnPremise
            services.AddOnPremiseInfrastructure(config);
#endif

#if OnAzure
            services.AddAzureInfrastructure(config);
            services.Scan(scan =>
            {
                scan.FromAssembliesOf(typeof(CustomersArchiveHandler))
                    .RegisterHandlers(typeof(IRequestHandler<>))
                    .RegisterHandlers(typeof(IRequestHandler<,>))
                    .RegisterHandlers(typeof(INotificationHandler<>));
            });
#endif
            return services
                .AddEventsService<Customer, Guid>()
                .AddEventsService<Account, Guid>();
        }

#if OnPremise

        private static IServiceCollection AddOnPremiseInfrastructure(this IServiceCollection services, IConfiguration config)
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
                .AddEventStore(eventstoreConnStr);
        }

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