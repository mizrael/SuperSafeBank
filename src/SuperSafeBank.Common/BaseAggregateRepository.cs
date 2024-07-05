using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Common;

public abstract class BaseAggregateRepository<TA, TKey> : IAggregateRepository<TA, TKey>
    where TA : class, IAggregateRoot<TKey>
{
    public async Task PersistAsync(TA aggregateRoot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregateRoot);

        if (!aggregateRoot.Events.Any())
            return;

        await PersistCoreAsync(aggregateRoot, cancellationToken).ConfigureAwait(false);

        aggregateRoot.ClearEvents();
    }

    protected abstract Task PersistCoreAsync(TA aggregateRoot, CancellationToken cancellationToken);

    public abstract Task<TA?> RehydrateAsync(TKey key, CancellationToken cancellationToken = default);
}