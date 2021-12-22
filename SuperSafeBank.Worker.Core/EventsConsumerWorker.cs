using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuperSafeBank.Common.EventBus;
using SuperSafeBank.Common.Models;
using SuperSafeBank.Domain;

namespace SuperSafeBank.Worker.Core
{
    public class EventsConsumerWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEnumerable<IEventConsumer> _consumers;

        public EventsConsumerWorker(IServiceScopeFactory eventConsumerFactory)
        {
            _scopeFactory = eventConsumerFactory;
            _consumers = new[]
            {
                Build<Account, Guid>(),
                Build<Customer, Guid>(),
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {            
            await Task.WhenAll(_consumers.Select(c => c.ConsumeAsync(stoppingToken)));
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            foreach(var consumer in _consumers)
                consumer.
            return base.StopAsync(cancellationToken);
        }

        private IEventConsumer Build<TA, TKey>() where TA : IAggregateRoot<TKey>
        {
            using var scope = _scopeFactory.CreateScope();
            var consumer = scope.ServiceProvider.GetRequiredService<IEventConsumer<TA, TKey>>();

            consumer.EventReceived += OnEventReceived<TKey>;

            return consumer;
        }

        private async Task OnEventReceived<TKey>(object s, IDomainEvent<TKey> e)
        {
            var @event = EventReceivedFactory.Create((dynamic)e);

            using var innerScope = _scopeFactory.CreateScope();
            var mediator = innerScope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Publish(@event, CancellationToken.None);
        }
    }
}
