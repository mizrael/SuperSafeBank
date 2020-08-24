using System;
using Microsoft.Azure.ServiceBus;

namespace SuperSafeBank.Persistence.Azure
{
    public class TopicClientFactory : ITopicClientFactory
    {
        private readonly string _connectionString;

        public TopicClientFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public ITopicClient Build(string topicName)
        {
            if (string.IsNullOrWhiteSpace(topicName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(topicName));
            return new TopicClient(_connectionString, topicName);
        }
    }
}