using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Common.EventBus;
using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Transport.Kafka
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddKafkaTransport(this IServiceCollection services, KafkaProducerConfig configuration)            
        {
            return services.AddSingleton(configuration)
                .AddSingleton<IEventProducer, EventProducer>();
        }
    }
}