using System.Threading.Tasks;
using SuperSafeBank.Core.Models;

namespace SuperSafeBank.Core
{
    public interface IEventsService<in TA, TKey> 
        where TA : class, IAggregateRoot<TKey>
    {
        Task PersistAsync(TA aggregateRoot);
    }
}