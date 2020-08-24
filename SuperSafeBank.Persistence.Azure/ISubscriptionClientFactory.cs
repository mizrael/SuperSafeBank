using Microsoft.Azure.ServiceBus;

namespace SuperSafeBank.Persistence.Azure
{
    public interface ISubscriptionClientFactory
    {
        ISubscriptionClient Build(string topicName, string subscriptionName);
    }
}