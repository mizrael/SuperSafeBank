using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using SuperSafeBank.Persistence.Azure.Tests.Models;

namespace SuperSafeBank.Persistence.Azure.Tests.Integration.Fixtures
{
    public class CosmosFixture : IDisposable
    {
        private readonly CosmosClient _client;
        private readonly List<Database> _dbs;
        private readonly string _eventsContainerName;
        private readonly string _dbNamePrefix;

        public CosmosFixture()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<CosmosFixture>()
                .Build();

            var connStr = configuration.GetConnectionString("cosmos");

            _eventsContainerName = configuration["eventsContainerName"];
            _dbNamePrefix = configuration["testDbName"];

            _client = new CosmosClient(connStr);
            _dbs = new List<Database>();
        }

        public async Task<IDbContainerProvider> CreateTestDatabaseAsync()
        {
            var dbName = $"{_dbNamePrefix}{Guid.NewGuid()}";
            var response = await _client.CreateDatabaseAsync(dbName);
            if (null == response || response.StatusCode != HttpStatusCode.Created)
                throw new Exception($"unable to create db {dbName}");

            await response.Database.CreateContainerIfNotExistsAsync(_eventsContainerName, $"/{nameof(DummyEvent.AggregateId)}");

            _dbs.Add(response.Database);
            return new DbContainerProvider(response.Database);
        }

        public void Dispose()
        {
            var tasks = _dbs.Select(db => db.DeleteAsync()).ToArray();
            Task.WaitAll(tasks);
            _client.Dispose();
        }
    }
}