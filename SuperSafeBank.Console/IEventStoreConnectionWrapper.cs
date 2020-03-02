using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace SuperSafeBank.Console
{
    public interface IEventStoreConnectionWrapper
    {
        Task<IEventStoreConnection> GetConnectionAsync();
    }
}