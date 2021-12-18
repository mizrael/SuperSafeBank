using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperSafeBank.Persistence.Azure.Tests.Integration.Fixtures
{
    public class StorageTableFixutre : Xunit.IAsyncLifetime
    {        
        private readonly Queue<TableClient> _tableClients = new();
        
        private readonly string _tablePrefix;
        private readonly string _connStr;

        public StorageTableFixutre()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddUserSecrets<StorageTableFixutre>(optional: true)
                .AddEnvironmentVariables()
                .Build();
            
            _connStr = configuration.GetConnectionString("storageTable");
            if(string.IsNullOrWhiteSpace(_connStr))
                throw new ArgumentException("invalid storage account connection string");

            _tablePrefix = configuration["tablePrefix"];
        }

        public async Task<TableClient> CreateTableClientAsync()
        {
            var client = new TableClient(_connStr, $"{_tablePrefix}DummyEvents{DateTime.UtcNow.Ticks}");
            await client.CreateIfNotExistsAsync();
            
            _tableClients.Enqueue(client);

            return client;
        }

        public async Task DisposeAsync()
        {
            while (_tableClients.Any())
            {
                await _tableClients.Dequeue().DeleteAsync();
            }
        }

        public Task InitializeAsync() => Task.CompletedTask;
    }
}