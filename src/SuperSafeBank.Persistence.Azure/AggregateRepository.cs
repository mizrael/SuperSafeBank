using Azure.Data.Tables;
using SuperSafeBank.Common;
using SuperSafeBank.Common.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSafeBank.Persistence.Azure
{
    public class AggregateRepository<TA, TKey>(TableClient tableClient, IEventSerializer eventDeserializer)
        : IAggregateRepository<TA, TKey>
        where TA : class, IAggregateRoot<TKey>
    {
        private readonly TableClient _client = tableClient ?? throw new ArgumentNullException(nameof(tableClient));
                
        private readonly IEventSerializer _eventSerializer = eventDeserializer ?? throw new ArgumentNullException(nameof(eventDeserializer));

        private static readonly ConcurrentDictionary<TKey, SemaphoreSlim> _locks = new();

        public async Task PersistAsync(TA aggregateRoot, CancellationToken cancellationToken = default)
        {
            if (aggregateRoot == null)
                throw new ArgumentNullException(nameof(aggregateRoot));

            if (!aggregateRoot.Events.Any())
                return;

            var firstEvent = aggregateRoot.Events.First();
            var expectedVersion = firstEvent.AggregateVersion;

            var aggregateLock = _locks.GetOrAdd(aggregateRoot.Id, (k) => new SemaphoreSlim(1, 1));
            await aggregateLock.WaitAsync(cancellationToken)
                                .ConfigureAwait(false);

            try
            {
                var prevAggregateEvents = _client.QueryAsync<EventData>(ed => ed.PartitionKey == aggregateRoot.Id.ToString() &&
                                                                                    ed.AggregateVersion >= expectedVersion, 
                                                                              cancellationToken: cancellationToken)
                                                .ConfigureAwait(false);

                await foreach (var @event in prevAggregateEvents)
                {
                    if (@event.AggregateVersion >= expectedVersion)
                        throw new ArgumentOutOfRangeException($"aggregate version mismatch, expected {expectedVersion}, got {@event.AggregateVersion}");
                }

                var newEvents = aggregateRoot.Events.Select(evt =>
                {
                    var eventData = EventData.Create(evt, _eventSerializer);
                    return new TableTransactionAction(TableTransactionActionType.Add, eventData);
                }).ToArray();

                await _client.SubmitTransactionAsync(newEvents, cancellationToken)
                            .ConfigureAwait(false);

                aggregateRoot.ClearEvents();
            }
            finally
            {
                aggregateLock.Release();
                _locks.Remove(aggregateRoot.Id, out _);
            }            
        }

        public async Task<TA> RehydrateAsync(TKey key, CancellationToken cancellationToken = default)
        {
            var aggregateEvents = _client.QueryAsync<EventData>(ed => ed.PartitionKey == key.ToString())
                                         .ConfigureAwait(false);

            var events = new List<IDomainEvent<TKey>>();

            await foreach (var @row in aggregateEvents) {
                var @event = _eventSerializer.Deserialize<TKey>(@row.EventType, @row.Data);
                events.Add(@event);
            }

            if (!events.Any())
                return null;

            var result = BaseAggregateRoot<TA, TKey>.Create(events.OrderBy(e => e.AggregateVersion));
            return result;
        }
    }
}