using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using SuperSafeBank.Core;
using SuperSafeBank.Core.Models;

namespace SuperSafeBank.Persistence.EventStore
{
    public class EventsRepository<TA, TKey> : IEventsRepository<TA, TKey>
        where TA : class, IAggregateRoot<TKey>
    {
        private readonly IEventStoreConnectionWrapper _connectionWrapper;
        private readonly string _streamBaseName;
        private readonly IEventDeserializer _eventDeserializer;

        public EventsRepository(IEventStoreConnectionWrapper connectionWrapper, IEventDeserializer eventDeserializer)
        {
            _connectionWrapper = connectionWrapper;
            _eventDeserializer = eventDeserializer;

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

            var streamName = GetStreamName(aggregateRoot.Id);

            var firstEvent = aggregateRoot.Events.First();
            var version = firstEvent.AggregateVersion - 1;

            using var transaction = await connection.StartTransactionAsync(streamName, version);

            try
            {
                foreach (var @event in aggregateRoot.Events)
                {
                    var eventData = Map(@event);
                    await transaction.WriteAsync(eventData);
                }

                await transaction.CommitAsync();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private string GetStreamName(TKey aggregateKey)
        {
            var streamName = $"{_streamBaseName}_{aggregateKey}";
            return streamName;
        }

        public async Task<TA> RehydrateAsync(TKey key)
        {
            var connection = await _connectionWrapper.GetConnectionAsync();
            
            var streamName = GetStreamName(key);

            var events = new List<IDomainEvent<TKey>>();

            StreamEventsSlice currentSlice;
            long nextSliceStart = StreamPosition.Start;
            do
            {
                currentSlice = await connection.ReadStreamEventsForwardAsync(streamName, nextSliceStart, 200, false);

                nextSliceStart = currentSlice.NextEventNumber;

                events.AddRange(currentSlice.Events.Select(Map));
            } while (!currentSlice.IsEndOfStream);

            var result = BaseAggregateRoot<TA, TKey>.Create(events.OrderBy(e => e.AggregateVersion));
            
            return result;
        }

        private IDomainEvent<TKey> Map(ResolvedEvent resolvedEvent)
        {
            var meta = System.Text.Json.JsonSerializer.Deserialize<EventMeta>(resolvedEvent.Event.Metadata);
            return _eventDeserializer.Deserialize<TKey>(meta.EventType, resolvedEvent.Event.Data);
        }

        private static EventData Map(IDomainEvent<TKey> @event)
        {
            var json = System.Text.Json.JsonSerializer.Serialize((dynamic) @event);
            var data = Encoding.UTF8.GetBytes(json);

            var eventType = @event.GetType();
            var meta = new EventMeta()
            {
                EventType = eventType.AssemblyQualifiedName
            };
            var metaJson = System.Text.Json.JsonSerializer.Serialize(meta);
            var metadata = Encoding.UTF8.GetBytes(metaJson);

            var eventPayload = new EventData(Guid.NewGuid(), eventType.Name, true, data, metadata);
            return eventPayload;
        }

        internal struct EventMeta
        {
            public string EventType { get; set; }
        }
    }

}