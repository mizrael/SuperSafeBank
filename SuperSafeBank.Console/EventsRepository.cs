using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using SuperSafeBank.Core.Models;

namespace SuperSafeBank.Console
{
    public class EventsRepository<TA, TKey> : IEventsRepository<TA, TKey>
        where TA : IAggregateRoot<TKey>
    {
        private readonly IEventStoreConnectionWrapper _connectionWrapper;
        private readonly string _streamBaseName;
        
        public EventsRepository(IEventStoreConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;

            var aggregateType = typeof(TA);
            _streamBaseName = aggregateType.Name;
        }

        public async Task AppendAsync(TA aggregateRoot)
        {
            if (null == aggregateRoot)
                throw new ArgumentNullException(nameof(aggregateRoot));

            if (!aggregateRoot.Events.Any())
                return;

            var connection = await _connectionWrapper.GetConnectionAsync();

            var streamName = $"{_streamBaseName}_{aggregateRoot.Id}";
            var firstEvent = aggregateRoot.Events.First();
            var version = firstEvent.AggregateVersion - 1;

            using var transaction =  await connection.StartTransactionAsync(streamName, version);
           
            try
            {
                foreach (var @event in aggregateRoot.Events)
                {
                    var eventData = Map(@event);
                    await transaction.WriteAsync(eventData);
                }

                await transaction.CommitAsync();

                aggregateRoot.ClearEvents();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
            }
        }

        public async Task<TA> Rehydrate(TKey key)
        {
            throw new NotImplementedException();
        }

        private EventData Map(IDomainEvent<TKey> @event)
        {
            var json = System.Text.Json.JsonSerializer.Serialize((dynamic) @event);

            var data = Encoding.UTF8.GetBytes(json);
            var metadata = Encoding.UTF8.GetBytes("{}");
            var eventType = @event.GetType();

            var eventPayload = new EventData(Guid.NewGuid(), eventType.Name, true, data, metadata);
            return eventPayload;
        }
    }
}