using SuperSafeBank.Common.Models;
using System.Data;
using Dapper;

namespace SuperSafeBank.Persistence.SQLServer
{
    public class AggregateTableCreator : IAggregateTableCreator
    {
        private readonly IDbConnection _dbConn;

        public AggregateTableCreator(IDbConnection dbConn, string schemaName = "aggregates")
        {
            if (string.IsNullOrWhiteSpace(schemaName))           
                throw new ArgumentException($"'{nameof(schemaName)}' cannot be null or whitespace.", nameof(schemaName));
            
            _dbConn = dbConn ?? throw new ArgumentNullException(nameof(dbConn));
            this.SchemaName = schemaName.ToLowerInvariant();
        }

        public async Task EnsureTableAsync<TA, TKey>(CancellationToken cancellationToken = default)
            where TA : class, IAggregateRoot<TKey>
        {
            //TODO: caching

            var tableName = this.GetTableName<TA, TKey>();

            var sql = $@"
            IF NOT EXISTS ( SELECT * FROM sys.schemas  WHERE name = N'{this.SchemaName}' ) BEGIN 
                EXEC('CREATE SCHEMA [{this.SchemaName}] AUTHORIZATION [DBO]');  
            END

            IF OBJECT_ID('{tableName}', 'U') IS NULL BEGIN
                CREATE TABLE {tableName} (
                    aggregateId nvarchar(250) NOT NULL,
                    aggregateVersion bigint NOT NULL,
                    eventType nvarchar(250) NULL,
                    data nvarchar(MAX) NOT NULL,                    
                    timestamp datetimeoffset NULL,
                    CONSTRAINT pk_{Guid.NewGuid().ToString("N")} PRIMARY KEY (aggregateId, aggregateVersion)
                );

                CREATE INDEX ix_{Guid.NewGuid().ToString("N")}_aggregateId ON {tableName} (aggregateId);
            END";

            await _dbConn.ExecuteAsync(sql).ConfigureAwait(false);
        }

        public string GetTableName<TA, TKey>()
            where TA : class, IAggregateRoot<TKey>
        {
            var aggregateType = typeof(TA);
            var aggregateName = aggregateType.Name;
            return $"{this.SchemaName}.{aggregateName}".ToLowerInvariant();
        }

        public string SchemaName { get; }
    }
}