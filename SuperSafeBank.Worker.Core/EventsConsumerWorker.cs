using Microsoft.Extensions.Hosting;
using SuperSafeBank.Common.EventBus;
using SuperSafeBank.Domain;

namespace SuperSafeBank.Worker.Core
{
    public class EventsConsumerWorker : BackgroundService
    {
        private readonly IEventConsumerFactory _eventConsumerFactory;

        public EventsConsumerWorker(IEventConsumerFactory eventConsumerFactory)
        {
            _eventConsumerFactory = eventConsumerFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IEnumerable<IEventConsumer> consumers = new[]
            {
                _eventConsumerFactory.Build<Account, Guid>(),
                _eventConsumerFactory.Build<Customer, Guid>(),
            };
            var tc = Task.WhenAll(consumers.Select(c => c.ConsumeAsync(stoppingToken)));
            await tc;
        }
    }
}
