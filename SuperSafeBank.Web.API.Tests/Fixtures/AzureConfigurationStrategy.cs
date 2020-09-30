
using System.Linq;
#if OnAzure
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.ServiceBus.Management;
using SuperSafeBank.Core.Models;
using SuperSafeBank.Domain;

namespace SuperSafeBank.Web.API.Tests.Fixtures
{
    internal class AzureConfigurationStrategy : IConfigurationStrategy, IDisposable
    {
        private string _cosmosConnStr;
        private string _cosmosDbName;
        private CosmosClient _cosmosClient;

        private string _serviceBusConnStr;
        private ManagementClient _serviceBusClient;
        private List<string> _topics;

        public void OnConfigureAppConfiguration(IConfigurationBuilder configurationBuilder)
        {
            _cosmosDbName = $"SuperSafeBank_tests_{Guid.NewGuid()}";
            var topicsBaseName = $"aggregate_tests_{Guid.NewGuid()}";

            _topics = new List<string>();

            configurationBuilder.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("dbName", _cosmosDbName),
                new KeyValuePair<string, string>("topicsBaseName", topicsBaseName)
            });

            var configuration = configurationBuilder.Build();

            _cosmosConnStr = configuration.GetConnectionString("cosmos");
            _cosmosClient = new CosmosClient(_cosmosConnStr);

            _serviceBusConnStr = configuration.GetConnectionString("producer");
            _serviceBusClient = new ManagementClient(_serviceBusConnStr);

            _topics.Add($"{topicsBaseName}-{typeof(Customer).Name}");
            _topics.Add($"{topicsBaseName}-{typeof(Account).Name}");

            var tasks = _topics.Select(topic => _serviceBusClient.CreateTopicAsync(topic))
                .Union(new Task[]
                {
                    _cosmosClient.CreateDatabaseAsync(_cosmosDbName)
                }).ToArray();
            Task.WaitAll(tasks);

            var db = _cosmosClient.GetDatabase(_cosmosDbName);

            tasks = _topics.Select(topic => _serviceBusClient.CreateSubscriptionAsync(topic, "created"))
                .Union(new Task[]
                {
                    db.CreateContainerAsync("Events", $"/{nameof(IDomainEvent<Guid>.AggregateId)}"),
                    db.CreateContainerAsync("CustomerEmails", "/id"),
                    db.CreateContainerAsync("CustomersArchive", "/id"),
                    db.CreateContainerAsync("CustomersDetails", "/id"),
                }).ToArray();
            Task.WaitAll(tasks);
        }

        public void Dispose()
        {
            var tasks = _topics.Select(topic => _serviceBusClient.DeleteTopicAsync(topic))
                .Union(new Task[]
                {
                    _cosmosClient.GetDatabase(_cosmosDbName).DeleteAsync(),

                }).ToArray();
            Task.WaitAll(tasks);
        }
    }
}

#endif
