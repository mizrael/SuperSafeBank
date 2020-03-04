using System.Threading.Tasks;
using SuperSafeBank.Core.Models;

namespace SuperSafeBank.Core
{
    public interface IEventsRepository<in TA, TKey>
        where TA : class, IAggregateRoot<TKey>
    {
        Task AppendAsync(TA aggregateRoot);
    }
}