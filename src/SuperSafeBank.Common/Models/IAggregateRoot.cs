using System.Collections.Generic;

namespace SuperSafeBank.Common.Models
{
    public interface IAggregateRoot<out TKey> : IEntity<TKey>
    {
        long Version { get; }

        /// <summary>
        /// list of events that have been applied but not persisted yet.
        /// </summary>
        IReadOnlyCollection<IDomainEvent<TKey>> NewEvents { get; }
    }
}