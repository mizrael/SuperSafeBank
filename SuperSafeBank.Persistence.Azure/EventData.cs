using System;
using Newtonsoft.Json;

namespace SuperSafeBank.Persistence.Azure
{
    internal class EventData<TKey>
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }
        public TKey AggregateId { get; set; }
        public long AggregateVersion { get; set; }
        public string Type { get; set; }
        public byte[] Data { get; set; }

        public static EventData<TKey> Create(TKey aggregateId, long aggregateVersion, string type, byte[] data)
        {
            if (data == null) 
                throw new ArgumentNullException(nameof(data));
            
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(type));

            return new EventData<TKey>()
            {
                Id = Guid.NewGuid(),
                AggregateId = aggregateId,
                AggregateVersion = aggregateVersion,
                Type = type,
                Data = data
            };
        }
    }
}