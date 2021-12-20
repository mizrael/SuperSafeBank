using System.Threading.Tasks;
using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Common.EventBus
{
    public interface IEventProducer<in TA, in TKey>
        where TA : IAggregateRoot<TKey>
    {
        Task DispatchAsync(TA aggregateRoot);
    }
}