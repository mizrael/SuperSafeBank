using Azure.Data.Tables;
using SuperSafeBank.Common;
using SuperSafeBank.Common.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSafeBank.Persistence.Azure;

public class StorageTableAggregateRepository<TA, TKey> : BaseAggregateRepository<TA, TKey>
    where TA : class, IAggregateRoot<TKey>
{
    private readonly TableClient _client;
            
    private readonly IEventSerializer _eventSerializer;

    private static readonly ConcurrentDictionary<TKey, SemaphoreSlim> _locks = new();

    public StorageTableAggregateRepository(TableClient tableClient, IEventSerializer eventDeserializer)
    {
        _client = tableClient ?? throw new ArgumentNullException(nameof(tableClient));
        _eventSerializer = eventDeserializer ?? throw new ArgumentNullException(nameof(eventDeserializer));         
    }

    protected override async Task PersistCoreAsync(TA aggregateRoot, CancellationToken cancellationToken = default)
    {
        var firstEvent = aggregateRoot.Events.First();
        var expectedVersion = firstEvent.AggregateVersion;

        //TODO: distributed locks
        //TODO: move the locking system to the base repository
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

            //TODO: clearing the domain events should happen here
        }
        finally
        {
            aggregateLock.Release();
            _locks.Remove(aggregateRoot.Id, out _);
        }            
    }

    public override async Task<TA> RehydrateAsync(TKey key, CancellationToken cancellationToken = default)
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