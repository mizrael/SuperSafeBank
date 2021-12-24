using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Common.EventBus;

namespace SuperSafeBank.Transport.Azure
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureTransport(this IServiceCollection services, IConfiguration config)
        {
            var topicName = config["topicsBaseName"];
            var connectionString = config.GetConnectionString("producer");

            return services.AddSingleton(ctx =>
                {                    
                    return new ServiceBusClient(connectionString);
                }).AddSingleton<IEventProducer>(ctx =>
                {
                    var clientFactory = ctx.GetRequiredService<ServiceBusClient>();
                    var logger = ctx.GetRequiredService<ILogger<EventProducer>>();
                    return new EventProducer(clientFactory, topicName, logger);
                });
        }
    }
}