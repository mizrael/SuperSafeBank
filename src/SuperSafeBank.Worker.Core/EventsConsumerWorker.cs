using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuperSafeBank.Common.EventBus;

namespace SuperSafeBank.Worker.Core
{
    public class EventsConsumerWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IEventConsumer _consumer;

        public EventsConsumerWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            _consumer = scope.ServiceProvider.GetRequiredService<IEventConsumer>();
            _consumer.EventReceived += this.OnEventReceived;
            await _consumer.StartConsumeAsync(stoppingToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            if(_consumer is not null)
                _consumer.EventReceived -= this.OnEventReceived;

            return base.StopAsync(cancellationToken);
        }

        private async Task OnEventReceived(object s, IIntegrationEvent @event)
        {
            using var innerScope = _scopeFactory.CreateScope();
            var mediator = innerScope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Publish(@event, CancellationToken.None);
        }
    }
}
