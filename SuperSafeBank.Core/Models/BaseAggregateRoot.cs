using System.Collections.Generic;
using System.Collections.Immutable;

namespace SuperSafeBank.Core.Models
{
    public abstract class BaseAggregateRoot<TA, TKey> : BaseEntity<TKey>, IAggregateRoot<TKey>
        where TA : class, IAggregateRoot<TKey>
    {
        protected BaseAggregateRoot(TKey id) : base(id)
        { 
            _events = new Queue<IDomainEvent<TKey>>();
        }

        private readonly Queue<IDomainEvent<TKey>> _events;

        public IReadOnlyCollection<IDomainEvent<TKey>> Events => _events.ToImmutableArray();

        public long Version { get; private set; }

        public void ClearEvents()
        {
            _events.Clear();
        }

        protected void AddEvent(IDomainEvent<TKey> @event)
        {
            _events.Enqueue(@event);
           
            this.Apply(@event);

            this.Version++;
        }

        protected abstract void Apply(IDomainEvent<TKey> @event);
    }
}