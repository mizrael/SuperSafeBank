using System;

namespace SuperSafeBank.Core.Models.Events
{
    public class AggregateCreated<TKey> : BaseDomainEvent<TKey> 
    {
        public AggregateCreated(DateTime when, TKey aggregateId) : base(aggregateId, when)
        {
        }
    }
}