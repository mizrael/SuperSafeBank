using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SuperSafeBank.Persistence.Kafka;
using SuperSafeBank.Web.API.Workers;

namespace SuperSafeBank.Web.API.Registries
{
    public static class EventConsumerRegistry
    {
        public static IServiceCollection RegisterWorker(this IServiceCollection services, IConfiguration config)
        {
            var kafkaConnStr = config.GetConnectionString("kafka");
            var eventsTopicName = config["eventsTopicName"];
            var groupName = config["eventsTopicGroupName"];
            var consumerConfig = new EventConsumerConfig(kafkaConnStr, eventsTopicName, groupName);
            services.AddSingleton(consumerConfig);

            services.AddSingleton(typeof(EventConsumer<,>));

            services.AddSingleton<IEventConsumerFactory, EventConsumerFactory>();

            services.AddHostedService(ctx =>
            {
                var factory = ctx.GetRequiredService<IEventConsumerFactory>();
                return new EventsConsumerWorker(factory);
            });

            return services;
        }
    }
}