using System;

namespace SuperSafeBank.Core.Models
{
    public interface IDomainEvent<out TA>
    {
        DateTime When { get; }
        TA AggregateId { get; }
    }

    public abstract class BaseDomainEvent<TKey> : IDomainEvent<TKey>
    {
        protected BaseDomainEvent(TKey aggregateId, DateTime when)
        {
            this.When = when;
            this.AggregateId = aggregateId ?? throw new ArgumentNullException(nameof(aggregateId));
        }

        public DateTime When { get; }
        public TKey AggregateId { get; }
    }
}