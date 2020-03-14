using System.Threading.Tasks;
using SuperSafeBank.Core.Models;

namespace SuperSafeBank.Core
{
    public interface IEventsService<TA, TKey> 
        where TA : class, IAggregateRoot<TKey>
    {
        Task PersistAsync(TA aggregateRoot);
        Task<TA> RehydrateAsync(TKey key);
    }
}