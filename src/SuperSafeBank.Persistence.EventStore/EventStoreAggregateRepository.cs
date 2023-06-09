﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using SuperSafeBank.Common;
using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Persistence.EventStore
{
    public class EventStoreAggregateRepository<TA, TKey> : IAggregateRepository<TA, TKey>
        where TA : class, IAggregateRoot<TKey>
    {
        private readonly IEventStoreConnectionWrapper _connectionWrapper;
        private readonly string _streamBaseName;
        private readonly IEventSerializer _eventDeserializer;

        public EventStoreAggregateRepository(IEventStoreConnectionWrapper connectionWrapper, IEventSerializer eventDeserializer)
        {
            _connectionWrapper = connectionWrapper;
            _eventDeserializer = eventDeserializer;

            var aggregateType = typeof(TA);
            _streamBaseName = aggregateType.Name;
        }

        public async Task PersistAsync(TA aggregateRoot, CancellationToken cancellationToken = default)
        {
            if (null == aggregateRoot)
                throw new ArgumentNullException(nameof(aggregateRoot));

            if (!aggregateRoot.Events.Any())
                return;
            
            var streamName = GetStreamName(aggregateRoot.Id);

            var firstEvent = aggregateRoot.Events.First();
            var version = firstEvent.AggregateVersion - 1;

            var connection = await _connectionWrapper.GetConnectionAsync().ConfigureAwait(false);
            using var transaction = await connection.StartTransactionAsync(streamName, version).ConfigureAwait(false);

            try
            {
                var newEvents = aggregateRoot.Events.Select(Map).ToArray();
                await transaction.WriteAsync(newEvents).ConfigureAwait(false);
                await transaction.CommitAsync().ConfigureAwait(false);
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            aggregateRoot.ClearEvents();
        }

        public async Task<TA> RehydrateAsync(TKey key, CancellationToken cancellationToken = default)
        {
            var connection = await _connectionWrapper.GetConnectionAsync().ConfigureAwait(false); ;
            
            var streamName = GetStreamName(key);

            var events = new List<IDomainEvent<TKey>>();

            StreamEventsSlice currentSlice;
            long nextSliceStart = StreamPosition.Start;
            do
            {
                currentSlice = await connection.ReadStreamEventsForwardAsync(streamName, nextSliceStart, 200, false)
                                               .ConfigureAwait(false);

                nextSliceStart = currentSlice.NextEventNumber;

                events.AddRange(currentSlice.Events.Select(Map));
            } while (!currentSlice.IsEndOfStream);

            if (!events.Any())
                return null;

            var result = BaseAggregateRoot<TA, TKey>.Create(events.OrderBy(e => e.AggregateVersion));
            
            return result;
        }

        private string GetStreamName(TKey aggregateKey)
            => $"{_streamBaseName}_{aggregateKey}";

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