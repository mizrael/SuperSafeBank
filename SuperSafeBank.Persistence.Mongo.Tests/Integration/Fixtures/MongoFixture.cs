using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;

namespace SuperSafeBank.Persistence.Mongo.Tests.Integration.Fixtures
{
    public class MongoFixture : IDisposable
    {
        private MongoClient _mongoClient;
        private readonly string _dbName;

        public IMongoDatabase Database { get; }        
        
        public MongoFixture()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connStr = configuration.GetConnectionString("mongo");

            _dbName = $"tests-{Guid.NewGuid()}";

            _mongoClient = new MongoClient(connectionString: connStr);
            this.Database = _mongoClient.GetDatabase(_dbName);            
        }

        public void Dispose()
        {
            if(null != Database)
            {
                _mongoClient.DropDatabase(_dbName);                
            }
        }
    }
}