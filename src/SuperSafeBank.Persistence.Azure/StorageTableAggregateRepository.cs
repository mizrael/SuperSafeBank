using Azure.Data.Tables;
using SuperSafeBank.Common;
using SuperSafeBank.Common.Models;
using System;
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

    public StorageTableAggregateRepository(TableClient tableClient, IEventSerializer eventDeserializer)
    {
        _client = tableClient ?? throw new ArgumentNullException(nameof(tableClient));
        _eventSerializer = eventDeserializer ?? throw new ArgumentNullException(nameof(eventDeserializer));         
    }

    protected override async Task PersistCoreAsync(TA aggregateRoot, CancellationToken cancellationToken = default)
    {
        var firstEvent = aggregateRoot.NewEvents.First();
        var expectedVersion = firstEvent.AggregateVersion;

        var prevAggregateEvents = _client.QueryAsync<EventData>(ed => ed.PartitionKey == aggregateRoot.Id.ToString() &&
                                                                            ed.AggregateVersion >= expectedVersion,
                                                                      cancellationToken: cancellationToken)
                                        .ConfigureAwait(false);

        await foreach (var @event in prevAggregateEvents)
        {
            if (@event.AggregateVersion >= expectedVersion)
                throw new ArgumentOutOfRangeException($"aggregate version mismatch, expected {expectedVersion}, got {@event.AggregateVersion}");
        }

        var newEvents = aggregateRoot.NewEvents.Select(evt =>
        {
            var eventData = EventData.Create(evt, _eventSerializer);
            return new TableTransactionAction(TableTransactionActionType.Add, eventData);
        }).ToArray();

        await _client.SubmitTransactionAsync(newEvents, cancellationToken)
                    .ConfigureAwait(false);
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