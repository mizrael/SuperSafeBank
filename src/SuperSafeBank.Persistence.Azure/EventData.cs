﻿using System;
using Azure;
using Azure.Data.Tables;
using SuperSafeBank.Common;
using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Persistence.Azure
{
    public record EventData : ITableEntity
    {
        /// <summary>
        /// this is the Aggregate id        
        /// </summary>
        public string PartitionKey { get; set; }
        
        /// <summary>
        /// aggregate version on the event
        /// </summary>
        public string RowKey { get; set; } 
        
        /// <summary>
        /// the event type
        /// </summary>
        public string EventType { get; init; }

        /// <summary>
        /// serialized event data
        /// </summary>
        public byte[] Data { get; init; }

        /// <summary>
        /// aggregate version on the event
        /// </summary>
        public long AggregateVersion { get; init; }
        
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public static EventData Create<TKey>(IDomainEvent<TKey> @event, IEventSerializer eventSerializer)
        {
            if (@event is null)
                throw new ArgumentNullException(nameof(@event));

            if (eventSerializer is null)
                throw new ArgumentNullException(nameof(eventSerializer));

            var data = eventSerializer.Serialize(@event);
            var eventType = @event.GetType();

            return new EventData()
            {                
                PartitionKey = @event.AggregateId.ToString(),
                RowKey = @event.AggregateVersion.ToString(),
                AggregateVersion = @event.AggregateVersion,
                EventType = eventType.AssemblyQualifiedName,
                Data = data
            };
        }
    }
}