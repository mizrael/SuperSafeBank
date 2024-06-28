using System.Threading;
using System.Threading.Tasks;
using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Common;

public interface IAggregateRepository<TA, TKey>
    where TA : class, IAggregateRoot<TKey>
{
    Task PersistAsync(TA aggregateRoot, CancellationToken cancellationToken = default);
    Task<TA?> RehydrateAsync(TKey key, CancellationToken cancellationToken = default);
}