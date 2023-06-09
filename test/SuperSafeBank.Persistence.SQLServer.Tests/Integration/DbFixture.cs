using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace SuperSafeBank.Persistence.SQLServer.Tests.Integration
{
    public class DbFixture : IAsyncLifetime
    {
        private readonly string _baseConnStr;
        private readonly Queue<string> _dbNames = new();
        private readonly static Random _rand = new();

        public DbFixture()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            _baseConnStr = configuration.GetConnectionString("sql");
            if (string.IsNullOrWhiteSpace(_baseConnStr))
                throw new ArgumentException("invalid connection string");

            SqlMapper.AddTypeHandler(new ByteArrayTypeHandler());
        }

        public async Task<SqlConnectionStringProvider> CreateDbConnectionStringProviderAsync()
        {
            var dbName = $"supersafebank_test_db_{Guid.NewGuid()}";
            var createDbConnStr = $"{_baseConnStr};Database=master";

            using var createDbConn = new SqlConnection(createDbConnStr);
            await createDbConn.OpenAsync();
            using var createDbCmd = new SqlCommand($"CREATE DATABASE [{dbName}];", createDbConn);
            await createDbCmd.ExecuteNonQueryAsync();

            _dbNames.Enqueue(dbName);

            var connectionString = $"{_baseConnStr};Database={dbName}";
            
            return new SqlConnectionStringProvider(connectionString);
        }

        public Task InitializeAsync()
            => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            var connStr = $"{_baseConnStr};Database=master;";
            using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            while (_dbNames.Any())
            {
                var dbName = _dbNames.Dequeue();

                try
                {
                    var dropDbSql = $"alter database [{dbName}] set single_user with rollback immediate; DROP DATABASE [{dbName}];";
                    using var dropCmd = new SqlCommand(dropDbSql, conn);
                    await dropCmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"unable to drop db '{dbName}' : {ex.Message}");
                }
            }
        }
    }
}
