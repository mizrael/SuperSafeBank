using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Core;
using SuperSafeBank.Persistence.Azure.Tests.Models;
using Xunit;

namespace SuperSafeBank.Persistence.Azure.Tests.Integration
{
    public class EventProducerTests : IDisposable
    {
        private static readonly JsonEventSerializer _eventSerializer;
        
        static EventProducerTests()
        {
            _eventSerializer = new JsonEventSerializer(new[] { typeof(DummyAggregate).Assembly });
        }

        private readonly string _connStr;
        private readonly IList<string> _topicNames = new List<string>();
        private const string SubscriptionName = nameof(EventProducerTests);
        private readonly ManagementClient _managementClient;

        public EventProducerTests()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<EventProducerTests>()
                .Build();

            _connStr = configuration.GetConnectionString("servicebus");

            _managementClient = new ManagementClient(_connStr);
        }

        [Fact]
        public async Task DispatchAsync_should_publish_events()
        {
            var topicFactory = new TopicClientFactory(_connStr);
            var logger = NSubstitute.Substitute.For<ILogger<EventProducer<DummyAggregate, Guid>>>();
            var sut = new EventProducer<DummyAggregate, Guid>(topicFactory, "test", _eventSerializer, logger);

            await CreateTopicAsync(sut.TopicName);

            int expectedMessagesCount = 3;

            var subscriptionClient = new SubscriptionClient(_connStr, sut.TopicName, SubscriptionName);
            subscriptionClient.RegisterMessageHandler(
                async (msg, tkn) =>
                {
                    --expectedMessagesCount;
                    await subscriptionClient.CompleteAsync(msg.SystemProperties.LockToken);
                },
                (ex) => Task.CompletedTask);

            var aggregate = new DummyAggregate(Guid.NewGuid());
            aggregate.DoSomething("lorem");
            aggregate.DoSomething("ipsum");

            await sut.DispatchAsync(aggregate);

            var timeout = 30 * 1000;
            var timer = new Stopwatch();
            timer.Start();
            while (true)
            {
                if (expectedMessagesCount < 1 || timer.ElapsedMilliseconds > timeout)
                    break;
                await Task.Delay(200);
            }

            expectedMessagesCount.Should().Be(0);
        }

        private async Task CreateTopicAsync(string topicName)
        {
            if (await _managementClient.TopicExistsAsync(topicName))
                return;
            
            await _managementClient.CreateTopicAsync(topicName);
            
            await _managementClient.CreateSubscriptionAsync(new SubscriptionDescription(topicName, SubscriptionName));
            
            _topicNames.Add(topicName);
        }

        public void Dispose()
        {
            var tt = _topicNames.Select(async t =>
                {
                    await _managementClient.DeleteSubscriptionAsync(t, SubscriptionName);

                    await _managementClient.GetTopicAsync(t);
                    
                    await _managementClient.DeleteTopicAsync(t);
                })
                .ToArray();
            Task.WaitAll(tt);
        }
    }
}