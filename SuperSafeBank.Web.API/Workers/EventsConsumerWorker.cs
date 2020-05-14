using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SuperSafeBank.Core.EventBus;

namespace SuperSafeBank.Web.API.Workers
{
    public class EventsConsumerWorker : BackgroundService
    {
        private readonly IEnumerable<IEventConsumer> _consumers;

        public EventsConsumerWorker(IEnumerable<IEventConsumer> consumers)
        {
            _consumers = consumers;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var tc = Task.WhenAll(_consumers.Select(c => c.ConsumeAsync(stoppingToken)));
            await tc;
        }
    }

}
