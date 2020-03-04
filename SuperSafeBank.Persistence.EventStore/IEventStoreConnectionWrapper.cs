using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace SuperSafeBank.Persistence.EventStore
{
    public interface IEventStoreConnectionWrapper
    {
        Task<IEventStoreConnection> GetConnectionAsync();
    }
}