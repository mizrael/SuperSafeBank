using System.Threading.Tasks;

namespace SuperSafeBank.Console
{
    public interface IEventsRepository<in TA, TKey>
    {
        Task AppendAsync(TA aggregateRoot);
    }
}