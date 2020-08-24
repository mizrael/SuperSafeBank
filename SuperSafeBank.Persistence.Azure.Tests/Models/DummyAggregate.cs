using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using SuperSafeBank.Core.Models;

namespace SuperSafeBank.Persistence.Azure.Tests.Models
{
    public class DummyAggregate : BaseAggregateRoot<DummyAggregate, Guid> {
        private DummyAggregate() { }
        public DummyAggregate(Guid id) : base(id)
        {
            this.AddEvent(new DummyEvent(this, "created"));
        }

        public void DoSomething(string what) => this.AddEvent(new DummyEvent(this, what));

        protected override void Apply(IDomainEvent<Guid> @event)
        {
            this.Id = @event.AggregateId;

            if (@event is DummyEvent dummyEvent)
                _whatHappened.Add(dummyEvent.Type);
        }

        private readonly IList<string> _whatHappened = new List<string>();
        public IReadOnlyCollection<string> WhatHappened => _whatHappened.ToImmutableList();
    }
}