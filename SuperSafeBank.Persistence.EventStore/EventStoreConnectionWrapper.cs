using System;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace SuperSafeBank.Persistence.EventStore
{
    public class EventStoreConnectionWrapper : IEventStoreConnectionWrapper, IDisposable
    {
        private readonly Lazy<Task<IEventStoreConnection>> _lazyConnection;
        private readonly Uri _connString;

        public EventStoreConnectionWrapper(Uri connString)
        {
            _connString = connString;

            _lazyConnection = new Lazy<Task<IEventStoreConnection>>(() =>
            {
                return Task.Run(async () =>
                {
                    var connection = EventStoreConnection.Create(_connString);
                    await connection.ConnectAsync();
                    return connection;
                });
            });
        }

        public Task<IEventStoreConnection> GetConnectionAsync()
        {
            return _lazyConnection.Value;
        }

        public void Dispose()
        {
            if (!_lazyConnection.IsValueCreated)
                return;

            _lazyConnection.Value.Result.Dispose();
        }
    }
}