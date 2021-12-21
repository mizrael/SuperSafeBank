using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Persistence.Tests.Models
{
    public record DummyAggregate : BaseAggregateRoot<DummyAggregate, Guid>
    {
        private DummyAggregate() { }
        public DummyAggregate(Guid id) : base(id)
        {
            Append(new DummyEvent(this, "created"));
        }

        public void DoSomething(string what) => Append(new DummyEvent(this, what));

        protected override void When(IDomainEvent<Guid> @event)
        {
            Id = @event.AggregateId;

            if (@event is DummyEvent dummyEvent)
                _whatHappened.Add(dummyEvent.Type);
        }

        private readonly IList<string> _whatHappened = new List<string>();
        public IReadOnlyCollection<string> WhatHappened => _whatHappened.ToImmutableList();
    }
}