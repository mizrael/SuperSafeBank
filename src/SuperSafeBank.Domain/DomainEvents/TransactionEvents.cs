using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Domain.DomainEvents;

public static class TransactionEvents
{
    public record StepForward : BaseDomainEvent<Transaction, Guid>
    {
        private StepForward() { }

        public StepForward(Transaction transaction) : base(transaction)
        {
            OldState = transaction.CurrentState;
        }

        public string OldState { get; init; }
    }

    public record TransactionCreated : BaseDomainEvent<Transaction, Guid>
    {
        private TransactionCreated() { }

        public TransactionCreated(Transaction transaction, string type, string[] states, IDictionary<string, string> properties) : base(transaction)
        {
            Type = type;
            States = states;
            Properties = properties.ToImmutableDictionary();
        }

        public string Type { get; init; }
        public string[] States { get; init; }
        public IReadOnlyDictionary<string, string> Properties { get; init; }
    }
}
