using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SuperSafeBank.Common.EventBus;
using SuperSafeBank.Common.Models;
using SuperSafeBank.Domain;

namespace SuperSafeBank.Worker.Core
{
    public static class EventConsumerRegistry
    {
        public static IServiceCollection RegisterWorker(this IServiceCollection services)
            => services.AddHostedService<EventsConsumerWorker>();
    }
}