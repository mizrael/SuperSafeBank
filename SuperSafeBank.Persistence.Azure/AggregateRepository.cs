using AsyncKeyedLock;
using Azure.Data.Tables;
using SuperSafeBank.Common;
using SuperSafeBank.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSafeBank.Persistence.Azure
{
    public class AggregateRepository<TA, TKey> : IAggregateRepository<TA, TKey>
        where TA : class, IAggregateRoot<TKey>
    {
        private readonly TableClient _client;
                
        private readonly IEventSerializer _eventSerializer;

        private static readonly AsyncKeyedLocker<TKey> _locks = new(o =>
        {
            o.PoolSize = 20;
            o.PoolInitialFill = 1;
        });

        public AggregateRepository(TableClient tableClient, IEventSerializer eventDeserializer)
        {
            _client = tableClient ?? throw new ArgumentNullException(nameof(tableClient));
            _eventSerializer = eventDeserializer ?? throw new ArgumentNullException(nameof(eventDeserializer));         
        }

        public async Task PersistAsync(TA aggregateRoot, CancellationToken cancellationToken = default)
        {
            if (aggregateRoot == null)
                throw new ArgumentNullException(nameof(aggregateRoot));

            if (!aggregateRoot.Events.Any())
                return;

            var firstEvent = aggregateRoot.Events.First();
            var expectedVersion = firstEvent.AggregateVersion;

            using (await _locks.LockAsync(aggregateRoot.Id, cancellationToken).ConfigureAwait(false))
            {
                var prevAggregateEvents = _client.QueryAsync<EventData<TKey>>(ed => ed.PartitionKey == aggregateRoot.Id.ToString() &&
                                                                                    ed.AggregateVersion >= expectedVersion)
                                                .ConfigureAwait(false);

                await foreach (var @event in prevAggregateEvents)
                {
                    if (@event.AggregateVersion >= expectedVersion)
                        throw new ArgumentOutOfRangeException($"aggregate version mismatch, expected {expectedVersion}, got {@event.AggregateVersion}");
                }

                var newEvents = aggregateRoot.Events.Select(evt =>
                {
                    var eventData = EventData<TKey>.Create(evt, _eventSerializer);
                    return new TableTransactionAction(TableTransactionActionType.Add, eventData);
                }).ToArray();

                await _client.SubmitTransactionAsync(newEvents);

                aggregateRoot.ClearEvents();
            }
        }

        public async Task<TA> RehydrateAsync(TKey key, CancellationToken cancellationToken = default)
        {
            var aggregateEvents = _client.QueryAsync<EventData<TKey>>(ed => ed.PartitionKey == key.ToString())
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