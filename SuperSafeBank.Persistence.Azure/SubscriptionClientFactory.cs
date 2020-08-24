using System;
using Microsoft.Azure.ServiceBus;

namespace SuperSafeBank.Persistence.Azure
{
    public class SubscriptionClientFactory : ISubscriptionClientFactory
    {
        private readonly string _connectionString;

        public SubscriptionClientFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public ISubscriptionClient Build(string topicName, string subscriptionName)
        {
            if (string.IsNullOrWhiteSpace(topicName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(topicName));
            return new SubscriptionClient(_connectionString, topicName, subscriptionName);
        }
    }
}