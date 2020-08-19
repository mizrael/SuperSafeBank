using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SuperSafeBank.Core.EventBus;
using SuperSafeBank.Persistence.Kafka;
using SuperSafeBank.Web.API.Workers;

namespace SuperSafeBank.Web.API.Registries
{
    public static class EventConsumerRegistry
    {
        public static IServiceCollection RegisterWorker(this IServiceCollection services, IConfiguration config)
        {
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