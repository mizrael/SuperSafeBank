using Dapper;
using Microsoft.Data.SqlClient;
using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Persistence.SQLServer
{
    public class AggregateTableCreator : IAggregateTableCreator
    {
        private readonly string _dbConnString;
        private readonly string _schemaName;
        private readonly HashSet<string> _cache = new();
        private readonly SemaphoreSlim _lock = new (1,1);

        public AggregateTableCreator(SqlConnectionStringProvider dbConnStringProvider, string schemaName = "aggregates")
        {            
            if (string.IsNullOrWhiteSpace(schemaName))           
                throw new ArgumentException($"'{nameof(schemaName)}' cannot be null or whitespace.", nameof(schemaName));
                        
            _schemaName = schemaName.ToLowerInvariant();
            _dbConnString = dbConnStringProvider.ConnectionString;
        }

        public async Task EnsureTableAsync<TA, TKey>(CancellationToken cancellationToken = default)
            where TA : class, IAggregateRoot<TKey>
        {
            var tableName = this.GetTableName<TA, TKey>();
            if (_cache.Contains(tableName))
                return;

            await _lock.WaitAsync(cancellationToken);

            try
            {
                if (_cache.Contains(tableName))
                    return;

                var sql = $@"
                    IF NOT EXISTS ( SELECT * FROM sys.schemas  WHERE name = N'{_schemaName}' ) BEGIN 
                        EXEC('CREATE SCHEMA [{_schemaName}] AUTHORIZATION [DBO]');  
                    END

                    IF OBJECT_ID('{tableName}', 'U') IS NULL BEGIN
                        CREATE TABLE {tableName} (
                            aggregateId nvarchar(250) NOT NULL,
                            aggregateVersion bigint NOT NULL,
                            eventType nvarchar(250) NULL,
                            data varchar(MAX) NOT NULL,                    
                            timestamp datetimeoffset NULL,
                            CONSTRAINT pk_{Guid.NewGuid().ToString("N")} PRIMARY KEY (aggregateId, aggregateVersion)
                        );

                        CREATE INDEX ix_{Guid.NewGuid().ToString("N")}_aggregateId ON {tableName} (aggregateId);
                    END";

                using var _dbConn = new SqlConnection(_dbConnString);
                await _dbConn.OpenAsync().ConfigureAwait(false);
                await _dbConn.ExecuteAsync(sql).ConfigureAwait(false);

                _cache.Add(tableName);
            }
            finally
            {
                _lock.Release();
            }
        }

        public string GetTableName<TA, TKey>()
            where TA : class, IAggregateRoot<TKey>
        {
            var aggregateType = typeof(TA);
            var aggregateName = aggregateType.Name;
            return $"{this._schemaName}.{aggregateName}".ToLowerInvariant();
        }
    }
}