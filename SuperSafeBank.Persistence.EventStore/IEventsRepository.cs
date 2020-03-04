using System.Threading.Tasks;

namespace SuperSafeBank.Persistence.EventStore
{
    public interface IEventsRepository<in TA, TKey>
    {
        Task AppendAsync(TA aggregateRoot);
    }
}