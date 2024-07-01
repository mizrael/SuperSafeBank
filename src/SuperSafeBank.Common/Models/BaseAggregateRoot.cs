﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace SuperSafeBank.Common.Models;

public abstract record BaseAggregateRoot<TA, TKey> : BaseEntity<TKey>, IAggregateRoot<TKey>
    where TA : class, IAggregateRoot<TKey>
{
    private readonly Queue<IDomainEvent<TKey>> _events = new Queue<IDomainEvent<TKey>>();

    protected BaseAggregateRoot() { }
    
    protected BaseAggregateRoot(TKey id) : base(id)
    {
    }

    public IReadOnlyCollection<IDomainEvent<TKey>> Events => _events.ToImmutableArray();

    public long Version { get; private set; }

    public DateTimeOffset LastUpdate => _events.Last().When;

    public void ClearEvents()
    {
        _events.Clear();
    }

    protected void Append(IDomainEvent<TKey> @event)
    {
        _events.Enqueue(@event);
       
        this.When(@event);

        this.Version++;
    }

    protected abstract void When(IDomainEvent<TKey> @event);

    #region Factory

    private static readonly ConstructorInfo CTor;

    static BaseAggregateRoot()
    {
        var aggregateType = typeof(TA);
        CTor = aggregateType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null, new Type[0], new ParameterModifier[0]);
        if (null == CTor)
            throw new InvalidOperationException($"Unable to find required private parameterless constructor for Aggregate of type '{aggregateType.Name}'");
    }

    public static TA Create(IEnumerable<IDomainEvent<TKey>> events)
    {
        if(null == events || !events.Any())
            throw new ArgumentNullException(nameof(events));
        var result = (TA)CTor.Invoke(new object[0]);

        var baseAggregate =  result as BaseAggregateRoot<TA, TKey>;
        if (baseAggregate != null) 
            foreach (var @event in events)
                baseAggregate.Append(@event);

        result.ClearEvents();

        return result;
    }

    #endregion Factory
}
