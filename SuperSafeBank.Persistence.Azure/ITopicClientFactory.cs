using Microsoft.Azure.ServiceBus;

namespace SuperSafeBank.Persistence.Azure
{
    public interface ITopicClientFactory
    {
        ITopicClient Build(string topicName);
    }
}