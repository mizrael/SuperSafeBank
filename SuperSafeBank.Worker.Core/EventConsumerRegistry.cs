using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SuperSafeBank.Common.EventBus;

namespace SuperSafeBank.Worker.Core
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