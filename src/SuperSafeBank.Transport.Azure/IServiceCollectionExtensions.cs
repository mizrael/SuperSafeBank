using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Common.EventBus;

namespace SuperSafeBank.Transport.Azure
{
    public record EventProducerConfig(string ConnectionString, string TopicName);

    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureTransport(this IServiceCollection services, EventProducerConfig config)
        {
            return services.AddSingleton(ctx =>
                {                    
                    return new ServiceBusClient(config.ConnectionString);
                }).AddSingleton<IEventProducer>(ctx =>
                {
                    var clientFactory = ctx.GetRequiredService<ServiceBusClient>();
                    var logger = ctx.GetRequiredService<ILogger<EventProducer>>();
                    return new EventProducer(clientFactory, config.TopicName, logger);
                });
        }
    }
}