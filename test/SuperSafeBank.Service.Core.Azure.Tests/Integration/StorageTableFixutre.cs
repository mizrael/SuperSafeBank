using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using SuperSafeBank.Common;
using SuperSafeBank.Common.Models;
using SuperSafeBank.Domain.DomainEvents;
using SuperSafeBank.Persistence.Azure;
using SuperSafeBank.Service.Core.Azure.Common.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperSafeBank.Service.Core.Azure.Tests
{
    public class StorageTableFixutre : Xunit.IAsyncLifetime
    {        
        private readonly Queue<TableClient> _tableClients = new();
        
        private readonly string _tablePrefix;
        private readonly string _connStr;

        private readonly IEventSerializer _eventSerializer;

        public StorageTableFixutre()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddUserSecrets<StorageTableFixutre>(optional: true)
                .AddEnvironmentVariables()
                .Build();

            _connStr = configuration.GetConnectionString("QueryModelsStorage");
            if (string.IsNullOrWhiteSpace(_connStr))
                throw new ArgumentException("invalid storage account connection string");

            _tablePrefix = configuration["tablePrefix"];

            _eventSerializer = new JsonEventSerializer(new[] { typeof(CustomerEvents.CustomerCreated).Assembly });
        }

        public IViewsContext CreateTableClient() 
        {
           var ctx = new ViewsContext(_connStr, $"{_tablePrefix}{DateTime.UtcNow.Ticks}");

            _tableClients.Enqueue(ctx.CustomersDetails);
            _tableClients.Enqueue(ctx.CustomersArchive);
            _tableClients.Enqueue(ctx.Accounts);

            return ctx;
        }

        public IAggregateRepository<TA, TKey> CreateRepository<TA, TKey>() where TA : class, IAggregateRoot<TKey>
        {
            var tableName = $"{_tablePrefix}{nameof(TA)}{DateTime.UtcNow.Ticks}";
            var client = new TableClient(_connStr, tableName);
            client.CreateIfNotExists();

            var repo = new StorageTableAggregateRepository<TA, TKey>(client, _eventSerializer);

            _tableClients.Enqueue(client);

            return repo;
        }

        public async Task DisposeAsync()
        {
            while (_tableClients.Any())
            {
                var tableClient = _tableClients.Dequeue();
                await tableClient.DeleteAsync();
            }
        }

        public Task InitializeAsync() => Task.CompletedTask;
    }
}