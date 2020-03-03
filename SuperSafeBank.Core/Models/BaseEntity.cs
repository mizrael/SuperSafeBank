using System;
using System.Collections.Generic;

namespace SuperSafeBank.Core.Models
{
    public abstract class BaseEntity<TKey> : IEntity<TKey>
    {
        protected BaseEntity() { }

        protected BaseEntity(TKey id) => Id = id;

        public TKey Id { get; protected set; }

        public override bool Equals(object obj)
        {
            var entity = obj as BaseEntity<TKey>;
            return entity != null &&
                   this.GetType() == entity.GetType() &&
                   EqualityComparer<TKey>.Default.Equals(Id, entity.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.GetType(), Id);
        }

        public static bool operator ==(BaseEntity<TKey> entity1, BaseEntity<TKey> entity2)
        {
            return EqualityComparer<BaseEntity<TKey>>.Default.Equals(entity1, entity2);
        }

        public static bool operator !=(BaseEntity<TKey> entity1, BaseEntity<TKey> entity2)
        {
            return !(entity1 == entity2);
        }
    }
}