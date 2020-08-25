using Microsoft.Azure.Cosmos;

namespace SuperSafeBank.Persistence.Azure
{
    public class DbContainerProvider : IDbContainerProvider
    {
        private readonly Database _db;

        public DbContainerProvider(Database db)
        {
            _db = db;
        }

        public Container GetContainer(string containerName) => _db.GetContainer(containerName);
    }
}