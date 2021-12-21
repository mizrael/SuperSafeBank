﻿using System.Threading;
using System.Threading.Tasks;
using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Common
{
    public interface IEventsRepository<TA, TKey>
        where TA : class, IAggregateRoot<TKey>
    {
        Task AppendAsync(TA aggregateRoot, CancellationToken cancellationToken = default);
        Task<TA> RehydrateAsync(TKey key, CancellationToken cancellationToken = default);
    }
}