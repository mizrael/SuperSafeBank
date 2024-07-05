using EvenireDB.Client;
using EvenireDB.Common;
using SuperSafeBank.Common;
using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Persistence.EvenireDB;

internal class AggregateRepository<TA, TKey> : BaseAggregateRepository<TA, TKey>
        where TA : class, IAggregateRoot<TKey>        
{
    private readonly IEventsClient _client;
    private readonly IEventSerializer _eventDeserializer;

    public AggregateRepository(IEventsClient client, IEventSerializer eventDeserializer)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _eventDeserializer = eventDeserializer ?? throw new ArgumentNullException(nameof(eventDeserializer));
    }

    protected override async Task PersistCoreAsync(TA aggregateRoot, CancellationToken cancellationToken = default)
    {
        if (aggregateRoot.Id is not Guid streamKey)
            throw new NotSupportedException("only Guid keys are currently supported.");

        string streamType = GetStreamType();
        var streamId = new StreamId(streamKey, streamType);

        var events = aggregateRoot.Events.Select(evt => Event.Create(evt, evt.GetType().FullName))
                                         .ToArray();

        await _client.AppendAsync(streamId, events, cancellationToken)
                    .ConfigureAwait(false);
    }

    public override async Task<TA?> RehydrateAsync(TKey key, CancellationToken cancellationToken = default)
    {
        if (key is not Guid streamKey)
            throw new NotSupportedException("only Guid keys are currently supported.");

        string streamType = GetStreamType();
        var streamId = new StreamId(streamKey, streamType);

        var events = new List<IDomainEvent<TKey>>();
        await foreach (var @event in _client.ReadAsync(streamId, StreamPosition.Start, Direction.Forward, cancellationToken).ConfigureAwait(false))
        {
            var mappedEvent = Map(@event);
            events.Add(mappedEvent);
        }

        if (!events.Any())
            return null;

        var result = BaseAggregateRoot<TA, TKey>.Create(events.OrderBy(e => e.AggregateVersion));

        return result;
    }

    private IDomainEvent<TKey> Map(Event resolvedEvent)
    => _eventDeserializer.Deserialize<TKey>(resolvedEvent.Type, resolvedEvent.Data.Span);

    private static string GetStreamType()
    => typeof(TA).Name;
}