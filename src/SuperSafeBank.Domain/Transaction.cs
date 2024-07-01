using System;
using System.Collections.Generic;
using SuperSafeBank.Common.Models;
using SuperSafeBank.Domain.DomainEvents;

namespace SuperSafeBank.Domain;

public record Transaction : BaseAggregateRoot<Transaction, Guid>
{
    private Transaction() { }

    public Transaction(Guid id, string type, string[] states, IDictionary<string, string> properties) : base(id)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(type, nameof(type));
        ArgumentNullException.ThrowIfNull(states, nameof(states));
        ArgumentNullException.ThrowIfNull(properties, nameof(properties));
        
        this.Append(new TransactionEvents.TransactionCreated(this, type, states, properties));
    }

    protected override void When(IDomainEvent<Guid> @event)
    {
        switch(@event)
        {
            case TransactionEvents.TransactionCreated tc:
                this.Type = tc.Type;
                this.States = tc.States;
                this.Properties = tc.Properties;
                break;
        }
    }

    public IReadOnlyDictionary<string, string> Properties { get; private set; }

    public string[] States { get; private set; }

    public string CurrentState { get; private set; }

    public bool IsCompleted => States.Length > 0 && States[States.Length-1] == CurrentState;

    public string Type { get; private set; }
}
