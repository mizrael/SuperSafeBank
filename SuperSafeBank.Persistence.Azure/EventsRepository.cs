using SuperSafeBank.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using SuperSafeBank.Core;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace SuperSafeBank.Persistence.Azure
{

    public class EventsRepository<TA, TKey> : IEventsRepository<TA, TKey>
        where TA : class, IAggregateRoot<TKey>
    {
        private readonly Container _container;
        private const string ContainerName = "Events";
        private readonly IEventSerializer _eventSerializer;

        public EventsRepository(CosmosClient client, string dbName, IEventSerializer eventDeserializer)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            if (string.IsNullOrWhiteSpace(dbName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(dbName));
            _eventSerializer = eventDeserializer ?? throw new ArgumentNullException(nameof(eventDeserializer));

            var database = client.GetDatabase(dbName);
            _container = database.GetContainer(ContainerName);
        }

        public async Task AppendAsync(TA aggregateRoot)
        {
            if (aggregateRoot == null)
                throw new ArgumentNullException(nameof(aggregateRoot));

            if (!aggregateRoot.Events.Any())
                return;

            var partitionKey = new PartitionKey(aggregateRoot.Id.ToString());

            var transaction = _container.CreateTransactionalBatch(partitionKey);
            
            foreach (var @event in aggregateRoot.Events)
            {
                var data = _eventSerializer.Serialize(@event);
                var eventType = @event.GetType();
                var eventData = EventData<TKey>.Create(aggregateRoot.Id, eventType.AssemblyQualifiedName, data);
                transaction.CreateItem(eventData);
            }

            using var response = await transaction.ExecuteAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception($"an error has occurred while persisting events for aggregate '{aggregateRoot.Id}' : {response.Diagnostics}");

            //todo check aggregate version
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

            var result = BaseAggregateRoot<TA, TKey>.Create(events.OrderBy(e => e.AggregateVersion));

            return result;
        }
    }

    internal class EventData<TKey>
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }
        public TKey AggregateId { get; set; }
        public string Type { get; set; }
        public byte[] Data { get; set; }

        public static EventData<TKey> Create(TKey aggregateId, string type, byte[] data)
        {
            if (data == null) 
                throw new ArgumentNullException(nameof(data));
            
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(type));

            return new EventData<TKey>()
            {
                Id = Guid.NewGuid(),
                AggregateId = aggregateId,
                Type = type,
                Data = data
            };
        }
    }
}