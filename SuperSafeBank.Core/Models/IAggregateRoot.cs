using System.Collections.Generic;

namespace SuperSafeBank.Core.Models
{
    public interface IAggregateRoot<TKey> : IEntity<TKey>
    {
        public long Version { get; }
        IReadOnlyCollection<IDomainEvent<TKey>> Events { get; }
        void ClearEvents();
    }
}