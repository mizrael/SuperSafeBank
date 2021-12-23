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
        {
            services.AddHostedService(provider =>
            {
                var consumers = new IEventConsumer[]
                {
                    ResolveConsumer<Customer, Guid>(provider),
                    ResolveConsumer<Account, Guid>(provider),
                };
                return new EventsConsumerWorker(consumers);
            });

            return services;
        }

        private static IEventConsumer ResolveConsumer<TA, TKey>(IServiceProvider provider) where TA : IAggregateRoot<TKey>
        {
            using var scope = provider.CreateScope();

            var consumer = scope.ServiceProvider.GetRequiredService<IEventConsumer<TA, TKey>>();

            consumer.EventReceived += async (s, e) =>
            {
                var @event = EventReceivedFactory.Create((dynamic)e);

                using var innerScope = provider.CreateScope();
                var mediator = innerScope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Publish(@event, CancellationToken.None);
            };

            return consumer;
        }
    }
}