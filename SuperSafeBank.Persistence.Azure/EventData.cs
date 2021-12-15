using System;
using Newtonsoft.Json;
using SuperSafeBank.Core;
using SuperSafeBank.Core.Models;

namespace SuperSafeBank.Persistence.Azure
{
    internal class EventData<TKey>
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; init; }
        public TKey AggregateId { get; init; }
        public long AggregateVersion { get; init; }
        public string Type { get; init; }
        public byte[] Data { get; set; }

        public static EventData<TKey> Create(IDomainEvent<TKey> @event, IEventSerializer eventSerializer)
        {
            if (@event is null)
                throw new ArgumentNullException(nameof(@event));

            if (eventSerializer is null)
                throw new ArgumentNullException(nameof(eventSerializer));

            var data = eventSerializer.Serialize(@event);
            var eventType = @event.GetType();

            return new EventData<TKey>()
            {
                Id = Guid.NewGuid(),
                AggregateId = @event.AggregateId,
                AggregateVersion = @event.AggregateVersion,
                Type = eventType.AssemblyQualifiedName,
                Data = data
            };
        }
    }
}