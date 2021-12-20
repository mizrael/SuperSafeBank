using System.Threading.Tasks;
using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Common
{
    public interface IEventsService<TA, TKey> 
        where TA : class, IAggregateRoot<TKey>
    {
        Task PersistAsync(TA aggregateRoot);
        Task<TA> RehydrateAsync(TKey key);
    }
}