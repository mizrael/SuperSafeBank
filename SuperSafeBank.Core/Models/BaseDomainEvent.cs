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

        /// <summary>
        /// every subclass should call this one
        /// TODO: note to future self: I don't like it and neither should you. Find a better way.
        /// </summary>
        /// <param name="aggregateRoot"></param>
        protected BaseDomainEvent(TA aggregateRoot)
        {
            if(aggregateRoot is null)
                throw new ArgumentNullException(nameof(aggregateRoot));

            this.AggregateVersion = aggregateRoot.Version;
            this.AggregateId = aggregateRoot.Id;
            this.Timestamp = DateTime.UtcNow;
        }

        public long AggregateVersion { get; private set; }
        public TKey AggregateId { get; private set; }
        public DateTime Timestamp { get; private set; }
    }
}