#if DebugOnPremise 
//TODO: move to Transport project and add tests

//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Diagnostics;
//using System.Linq;
//using System.Threading.Tasks;
//using Azure.Messaging.ServiceBus;
//using FluentAssertions;
//using Microsoft.Azure.Management.ServiceBus;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using SuperSafeBank.Core;
//using SuperSafeBank.Persistence.Tests.Models;
//using Xunit;

//namespace SuperSafeBank.Persistence.Azure.Tests.Integration
//{
//    [Trait("Category", "Integration")]
//    [Category("Integration")]
//    public class EventProducerTests : IAsyncDisposable
//    {
//        private static readonly JsonEventSerializer _eventSerializer;

//        static EventProducerTests()
//        {
//            _eventSerializer = new JsonEventSerializer(new[] { typeof(DummyAggregate).Assembly });
//        }

//        private readonly ServiceBusConnectionStringProperties _connStr;
//        private readonly IList<string> _topicNames = new List<string>();
//        private const string SubscriptionName = nameof(EventProducerTests);
//        private readonly ServiceBusManagementClient _sbClient;

//        public EventProducerTests()
//        {
//            var configuration = new ConfigurationBuilder()
//                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
//                .AddUserSecrets<EventProducerTests>()
//                .AddEnvironmentVariables()
//                .Build();

//            var connStr = configuration.GetConnectionString("servicebus");            
//            if (string.IsNullOrWhiteSpace(connStr))
//                throw new ArgumentException("invalid servicebus connection string");

//            _connStr = ServiceBusConnectionStringProperties.Parse(connectionString: connStr);

//            _sbClient = new ServiceBusManagementClient((connStr);
//        }

//        [Fact]
//        public async Task DispatchAsync_should_publish_events()
//        {
//            var logger = NSubstitute.Substitute.For<ILogger<EventProducer<DummyAggregate, Guid>>>();
//            var sut = new EventProducer<DummyAggregate, Guid>(_sbClient, "test", _eventSerializer, logger);

//            await CreateTopicAsync(sut.TopicName);

//            int expectedMessagesCount = 3;

//            var subscriptionClient = new SubscriptionClient(_connStr, sut.TopicName, SubscriptionName);
//            subscriptionClient.RegisterMessageHandler(
//                async (msg, tkn) =>
//                {
//                    --expectedMessagesCount;
//                    await subscriptionClient.CompleteAsync(msg.SystemProperties.LockToken);
//                },
//                (ex) => Task.CompletedTask);

//            var aggregate = new DummyAggregate(Guid.NewGuid());
//            aggregate.DoSomething("lorem");
//            aggregate.DoSomething("ipsum");

//            await sut.DispatchAsync(aggregate);

//            var timeout = 30 * 1000;
//            var timer = new Stopwatch();
//            timer.Start();
//            while (true)
//            {
//                if (expectedMessagesCount < 1 || timer.ElapsedMilliseconds > timeout)
//                    break;
//                await Task.Delay(200);
//            }

//            expectedMessagesCount.Should().Be(0);
//        }

//        private async Task CreateTopicAsync(string topicName)
//        {
//            await _sbClient.Topics.CreateOrUpdateAsync(topicName);
//            _sbClient.Subscriptions.CreateOrUpdateAsync()
//            await _sbClient.CreateSubscriptionAsync(new SubscriptionDescription(topicName, SubscriptionName));

//            _topicNames.Add(topicName);
//        }

//        private static async Task<string> GetTokenAsync()
//        {
//            try
//            {
//                // Check to see if the token has expired before requesting one.
//                // We will go ahead and request a new one if we are within 2 minutes of the token expiring.
//                if (tokenExpiresAtUtc < DateTime.UtcNow.AddMinutes(-2))
//                {
//                    Console.WriteLine("Renewing token...");

//                    var tenantId = appOptions.TenantId;
//                    var clientId = appOptions.ClientId;
//                    var clientSecret = appOptions.ClientSecret;

//                    var context = new AuthenticationContext($"https://login.microsoftonline.com/{tenantId}");

//                    var result = await context.AcquireTokenAsync(
//                        "https://management.core.windows.net/",
//                        new ClientCredential(clientId, clientSecret)
//                    );

//                    // If the token isn't a valid string, throw an error.
//                    if (string.IsNullOrEmpty(result.AccessToken))
//                    {
//                        throw new Exception("Token result is empty!");
//                    }

//                    tokenExpiresAtUtc = result.ExpiresOn.UtcDateTime;
//                    tokenValue = result.AccessToken;
//                    Console.WriteLine("Token renewed successfully.");
//                }

//                return tokenValue;
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine("Could not get a new token...");
//                Console.WriteLine(e.Message);
//                throw e;
//            }
//        }
//    }

//    public async ValueTask DisposeAsync()
//        {
//            var tt = _topicNames.Select(async t =>
//                {
//                    await _sbClient.DeleteSubscriptionAsync(t, SubscriptionName);

//                    await _sbClient.GetTopicAsync(t);

//                    await _sbClient.DeleteTopicAsync(t);
//                })
//                .ToArray();
//            await Task.WhenAll(tt);
//        }
//    }
//}
#endif