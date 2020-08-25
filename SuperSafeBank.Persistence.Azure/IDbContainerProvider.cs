using Microsoft.Azure.Cosmos;

namespace SuperSafeBank.Persistence.Azure
{
    public interface IDbContainerProvider
    {
        Container GetContainer(string containerName);
    }
}