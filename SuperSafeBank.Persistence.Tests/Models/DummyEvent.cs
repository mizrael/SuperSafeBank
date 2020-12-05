using System;
using SuperSafeBank.Core.Models;

namespace SuperSafeBank.Persistence.Tests.Models
{
    public class DummyEvent : BaseDomainEvent<DummyAggregate, Guid>
    {
        private DummyEvent() { }
        public DummyEvent(DummyAggregate aggregate, string type) : base(aggregate)
        {
            Type = type;
        }

        public string Type { get; private set; }
    }
}