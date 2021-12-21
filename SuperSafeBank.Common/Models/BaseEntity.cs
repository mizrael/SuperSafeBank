using System;
using System.Collections.Generic;

namespace SuperSafeBank.Common.Models
{
    public abstract record BaseEntity<TKey> : IEntity<TKey>
    {
        protected BaseEntity() { }

        protected BaseEntity(TKey id) => Id = id;

        public TKey Id { get; protected set; }
    }
}