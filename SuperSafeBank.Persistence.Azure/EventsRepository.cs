using SuperSafeBank.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using SuperSafeBank.Core;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace SuperSafeBank.Persistence.Azure
{
    public class EventsRepository<TA, TKey> : IEventsRepository<TA, TKey>
        where TA : class, IAggregateRoot<TKey>
    {
        private readonly Container _container;
        private const string EventsContainerName = "Events";
        
        private readonly IEventSerializer _eventSerializer;

        private static readonly ConcurrentDictionary<TKey, SemaphoreSlim> _locks = new ConcurrentDictionary<TKey, SemaphoreSlim>();

        public EventsRepository(IDbContainerProvider containerProvider, IEventSerializer eventDeserializer)
        {
            if (containerProvider == null)
                throw new ArgumentNullException(nameof(containerProvider));
            
            _eventSerializer = eventDeserializer ?? throw new ArgumentNullException(nameof(eventDeserializer));

            _container = containerProvider.GetContainer(EventsContainerName);
        }

        public async Task AppendAsync(TA aggregateRoot)
        {
            if (aggregateRoot == null)
                throw new ArgumentNullException(nameof(aggregateRoot));

            if (!aggregateRoot.Events.Any())
                return;

            var partitionKey = new PartitionKey(aggregateRoot.Id.ToString());

            var firstEvent = aggregateRoot.Events.First();
            var expectedVersion = firstEvent.AggregateVersion;

            var aggregateLock = _locks.GetOrAdd(aggregateRoot.Id, (k) => new SemaphoreSlim(1, 1));
            await aggregateLock.WaitAsync();

            try
            {
                var dbVersionResp = await _container.GetItemLinqQueryable<EventData<TKey>>(
                        requestOptions: new QueryRequestOptions()
                        {
                            PartitionKey = partitionKey
                        }).Select(e => e.AggregateVersion)
                    .MaxAsync();
                if (null != dbVersionResp && dbVersionResp.StatusCode == HttpStatusCode.OK)
                {
                    if (dbVersionResp.Resource != expectedVersion)
                        throw new AggregateException(
                            $"aggregate version mismatch, expected {expectedVersion} , got {dbVersionResp.Resource}");
                }

                var transaction = _container.CreateTransactionalBatch(partitionKey);

                foreach (var @event in aggregateRoot.Events)
                {
                    var data = _eventSerializer.Serialize(@event);
                    var eventType = @event.GetType();
                    var eventData = EventData<TKey>.Create(aggregateRoot.Id, aggregateRoot.Version,
                        eventType.AssemblyQualifiedName, data);
                    transaction.CreateItem(eventData);
                }

                using var response = await transaction.ExecuteAsync();
                if (!response.IsSuccessStatusCode)
                    throw new Exception(
                        $"an error has occurred while persisting events for aggregate '{aggregateRoot.Id}' : {response.Diagnostics}");
            }
            finally
            {
                aggregateLock.Release();
            }

            _locks.Remove(aggregateRoot.Id, out _);
        }

        public async Task<TA> RehydrateAsync(TKey key)
        {
            var partitionKey = new PartitionKey(key.ToString());

            var events = new List<IDomainEvent<TKey>>();

            using var setIterator = _container.GetItemQueryIterator<EventData<TKey>>(requestOptions: new QueryRequestOptions { MaxItemCount = 100, PartitionKey = partitionKey });
            while (setIterator.HasMoreResults)
            {
                foreach (var item in await setIterator.ReadNextAsync())
                {
                    var @event = _eventSerializer.Deserialize<TKey>(item.Type, item.Data);
                    events.Add(@event);
                }
            }

            if (!events.Any())
                return null;

            var result = BaseAggregateRoot<TA, TKey>.Create(events.OrderBy(e => e.AggregateVersion));
            return result;
        }
    }
}