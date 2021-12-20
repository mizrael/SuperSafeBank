#if OnAzure
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;
using SuperSafeBank.Common.Models;
using SuperSafeBank.Domain;

namespace SuperSafeBank.Service.Core.Tests.Fixtures
{
    internal class AzureConfigurationStrategy : IConfigurationStrategy, IDisposable
    {
        private string _cosmosConnStr;
        private string _cosmosDbName;
        private CosmosClient _cosmosClient;
        private Database _db;

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
            if (string.IsNullOrWhiteSpace(_cosmosConnStr))
                throw new ArgumentException("invalid cosmos connection string");
            _cosmosClient = new CosmosClient(_cosmosConnStr);

            _serviceBusConnStr = configuration.GetConnectionString("producer");
            if (string.IsNullOrWhiteSpace(_serviceBusConnStr))
                throw new ArgumentException("invalid servicebus producer connection string");

            _serviceBusClient = new ManagementClient(_serviceBusConnStr);

            _topics.Add($"{topicsBaseName}-{typeof(Customer).Name}");
            _topics.Add($"{topicsBaseName}-{typeof(Account).Name}");

            var tasks = _topics.Select(topic => _serviceBusClient.CreateTopicAsync(topic))
                .Union(new Task[]
                {
                    _cosmosClient.CreateDatabaseAsync(_cosmosDbName)
                }).ToArray();
            Task.WaitAll(tasks);

            _db = _cosmosClient.GetDatabase(_cosmosDbName);

            tasks = _topics.Select(topic => _serviceBusClient.CreateSubscriptionAsync(topic, "created"))
                .Union(new Task[]
                {
                    _db.CreateContainerAsync("Events", $"/{nameof(IDomainEvent<Guid>.AggregateId)}"),
                    _db.CreateContainerAsync("CustomerEmails", "/id"),
                    _db.CreateContainerAsync("CustomersArchive", "/id"),
                    _db.CreateContainerAsync("CustomersDetails", "/id"),
                    _db.CreateContainerAsync("AccountsDetails", "/id"),
                }).ToArray();
            Task.WaitAll(tasks);
        }

        public void Dispose()
        {
            var tasks = _topics.Select(topic => _serviceBusClient.DeleteTopicAsync(topic))
                .Union(new Task[]
                {
                    _db.DeleteAsync()
                }).ToArray();
            Task.WaitAll(tasks);
        }

        public IQueryModelsSeeder CreateSeeder() => new AzureQueryModelsSeeder(_db);
    }
}

#endif
