using Microsoft.Extensions.DependencyInjection;

namespace SuperSafeBank.Worker.Core.Registries
{
    public static class EventConsumerRegistry
    {
        public static IServiceCollection RegisterWorker(this IServiceCollection services)
            => services.AddHostedService<EventsConsumerWorker>();
    }
}