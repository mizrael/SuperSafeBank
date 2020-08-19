using Microsoft.Extensions.DependencyInjection;
using SuperSafeBank.Core.EventBus;

namespace SuperSafeBank.Persistence.Kafka
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection RegisterKafkaConsumer(this IServiceCollection services, EventConsumerConfig consumerConfig)
        {
            return services.AddSingleton(consumerConfig)
                .AddSingleton(typeof(IEventConsumer<,>), typeof(EventConsumer<,>));
        }
    }
}