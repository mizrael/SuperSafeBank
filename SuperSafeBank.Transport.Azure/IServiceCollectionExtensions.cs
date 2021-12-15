using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Core;
using SuperSafeBank.Core.EventBus;
using SuperSafeBank.Core.Models;
using SuperSafeBank.Domain;
using System;

namespace SuperSafeBank.Transport.Azure
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureTransport(this IServiceCollection services, IConfiguration config)
        {
            return services.AddSingleton(ctx =>
                {
                    var connectionString = config.GetConnectionString("producer");
                    return new ServiceBusClient(connectionString);
                }).AddEventsProducer<Customer, Guid>(config)
                .AddEventsProducer<Account, Guid>(config);
        }
        private static IServiceCollection AddEventsProducer<TA, TK>(this IServiceCollection services, IConfiguration config)
            where TA : class, IAggregateRoot<TK>
        {
            var topicsBaseName = config["topicsBaseName"];
            return services.AddSingleton<IEventProducer<TA, TK>>(ctx =>
            {
                var clientFactory = ctx.GetRequiredService<ServiceBusClient>();
                var eventDeserializer = ctx.GetRequiredService<IEventSerializer>();
                var logger = ctx.GetRequiredService<ILogger<EventProducer<TA, TK>>>();
                return new EventProducer<TA, TK>(clientFactory, topicsBaseName, eventDeserializer, logger);
            });
        }
    }
}