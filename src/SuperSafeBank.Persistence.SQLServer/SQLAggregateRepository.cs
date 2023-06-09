using Dapper;
using SuperSafeBank.Common;
using SuperSafeBank.Common.Models;
using System.Data;

namespace SuperSafeBank.Persistence.SQLServer
{
    public class SQLAggregateRepository<TA, TKey> : IAggregateRepository<TA, TKey>
        where TA : class, IAggregateRoot<TKey>
    {
        private readonly IDbConnection _dbConn;
        private readonly IAggregateTableCreator _tableCreator;
        private readonly IEventSerializer _eventSerializer;

        public SQLAggregateRepository(
            IDbConnection dbConn, 
            IAggregateTableCreator tableCreator,
            IEventSerializer eventSerializer)
        {
            _dbConn = dbConn ?? throw new ArgumentNullException(nameof(dbConn));
            _tableCreator = tableCreator ?? throw new ArgumentNullException(nameof(tableCreator));
            _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));

            // TODO: move to DI
            SqlMapper.AddTypeHandler(new ByteArrayTypeHandler());
        }

        public async Task PersistAsync(TA aggregateRoot, CancellationToken cancellationToken = default)
        {
            if (aggregateRoot is null)
                throw new ArgumentNullException(nameof(aggregateRoot));
            if (!aggregateRoot.Events.Any())
                return;

            await _tableCreator.EnsureTableAsync<TA, TKey>(cancellationToken)
                               .ConfigureAwait(false);

            using var transaction = _dbConn.BeginTransaction(IsolationLevel.Serializable);
            
            try
            {
                var lastVersion = await this.GetLastAggregateVersionAsync(transaction)
                                  .ConfigureAwait(false);
                if (lastVersion >= aggregateRoot.Version)
                    throw new ArgumentOutOfRangeException($"aggregate version mismatch, expected {aggregateRoot.Version}, got {lastVersion}");

                var tableName = _tableCreator.GetTableName<TA, TKey>();
                var sql = $@"INSERT INTO {tableName} (aggregateId, aggregateVersion, eventType, data, timestamp)
                         VALUES (@aggregateId, @aggregateVersion, @eventType, @data, @timestamp);";

                var entities = aggregateRoot.Events.Select(evt => AggregateEvent.Create(evt, _eventSerializer))
                                                   .ToList();
                await _dbConn.ExecuteAsync(sql, param: entities, transaction: transaction)
                             .ConfigureAwait(false);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<TA> RehydrateAsync(TKey key, CancellationToken cancellationToken = default)
        {
            await _tableCreator.EnsureTableAsync<TA, TKey>(cancellationToken)
                               .ConfigureAwait(false);

            var tableName = _tableCreator.GetTableName<TA, TKey>();
            var sql = $@"SELECT aggregateId, aggregateVersion, eventType, data, timestamp
                         FROM {tableName}
                         WHERE aggregateId = @aggregateId
                         ORDER BY aggregateVersion ASC";
                        
            var aggregateEvents = await _dbConn.QueryAsync<AggregateEvent>(sql, new { aggregateId = key })
                                                .ConfigureAwait(false);
            if (aggregateEvents?.Any() == false)
                return null; 
            
            var events = new List<IDomainEvent<TKey>>();

            foreach (var aggregateEvent in aggregateEvents)
            {
                var @event = _eventSerializer.Deserialize<TKey>(aggregateEvent.EventType, aggregateEvent.Data);
                events.Add(@event);
            }            

            var result = BaseAggregateRoot<TA, TKey>.Create(events.OrderBy(e => e.AggregateVersion));
            return result;
        }

        private async Task<long?> GetLastAggregateVersionAsync(IDbTransaction transaction)
        {
            var tableName = _tableCreator.GetTableName<TA, TKey>();
            var sql = $"SELECT TOP 1 aggregateVersion FROM {tableName} ORDER BY aggregateVersion DESC";
            var result = await _dbConn.QueryFirstOrDefaultAsync<long?>(sql, transaction: transaction)
                                      .ConfigureAwait(false);
            return result;
        }
    }
}