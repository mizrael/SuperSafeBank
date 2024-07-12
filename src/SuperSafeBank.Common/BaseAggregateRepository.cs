using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using SuperSafeBank.Common.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SuperSafeBank.Common;

public abstract class BaseAggregateRepository<TA, TKey> : IAggregateRepository<TA, TKey>
    where TA : class, IAggregateRoot<TKey>
    where TKey: notnull
{
    private static readonly ConcurrentDictionary<TKey, SemaphoreSlim> _locks = new();

    public async Task PersistAsync(TA aggregateRoot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregateRoot);

        if (!aggregateRoot.NewEvents.Any())
            return;

        //TODO: distributed locking
        var aggregateLock = _locks.GetOrAdd(aggregateRoot.Id, (k) => new SemaphoreSlim(1, 1));
        await aggregateLock.WaitAsync(cancellationToken)
                            .ConfigureAwait(false);

        try
        {
            await PersistCoreAsync(aggregateRoot, cancellationToken).ConfigureAwait(false);

            if (aggregateRoot is BaseAggregateRoot<TA, TKey> bar)
                bar.ClearEvents();
        }
        finally
        {
            aggregateLock.Release();
            _locks.Remove(aggregateRoot.Id, out _);
        }
    }

    protected abstract Task PersistCoreAsync(TA aggregateRoot, CancellationToken cancellationToken);

    public abstract Task<TA?> RehydrateAsync(TKey key, CancellationToken cancellationToken = default);
}