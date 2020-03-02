using System;

namespace SuperSafeBank.Core.Models
{
    public abstract class BaseDomainEvent<TA, TKey> : IDomainEvent<TKey>
        where TA : IAggregateRoot<TKey>
    {
        /// <summary>
        /// for deserialization
        /// </summary>
        protected BaseDomainEvent() { }

        protected BaseDomainEvent(TA aggregateRoot)
        {
            if(aggregateRoot is null)
                throw new ArgumentNullException(nameof(aggregateRoot));

            this.AggregateVersion = aggregateRoot.Version;
            this.AggregateId = aggregateRoot.Id;
        }

        public long AggregateVersion { get; private set; }
        public TKey AggregateId { get; private set; }
    }
}