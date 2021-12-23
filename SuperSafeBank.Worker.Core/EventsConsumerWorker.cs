using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuperSafeBank.Common.EventBus;
using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Worker.Core
{
    public class EventsConsumerWorker : BackgroundService
    {
        private readonly IEnumerable<IEventConsumer> _consumers;

        public EventsConsumerWorker(IEnumerable<IEventConsumer> consumers)
        {
            _consumers = consumers ?? throw new ArgumentNullException(nameof(consumers));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {            
            await Task.WhenAll(_consumers.Select(c => c.ConsumeAsync(stoppingToken)));
        }


    }
}
