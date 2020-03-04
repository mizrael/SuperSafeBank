using System.Threading.Tasks;
using SuperSafeBank.Core.Models;

namespace SuperSafeBank.Console.EventBus
{
    public interface IEventProducer<in TA, in TKey>
        where TA : IAggregateRoot<TKey>
    {
        Task DispatchAsync(TA aggregateRoot);
    }
}