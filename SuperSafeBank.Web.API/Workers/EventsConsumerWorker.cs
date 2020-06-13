using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Core;
using SuperSafeBank.Core.EventBus;
using SuperSafeBank.Domain;

namespace SuperSafeBank.Web.API.Workers
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
