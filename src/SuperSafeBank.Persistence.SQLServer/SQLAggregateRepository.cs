using Dapper;
using Microsoft.Data.SqlClient;
using SuperSafeBank.Common;
using SuperSafeBank.Common.Models;
using System.Data;

namespace SuperSafeBank.Persistence.SQLServer;

public class SQLAggregateRepository<TA, TKey> : BaseAggregateRepository<TA, TKey>
    where TA : class, IAggregateRoot<TKey>
{
    private readonly string _dbConnString;
    private readonly IAggregateTableCreator _tableCreator;
    private readonly IEventSerializer _eventSerializer;

    public SQLAggregateRepository(
        SqlConnectionStringProvider connectionStringProvider, 
        IAggregateTableCreator tableCreator,
        IEventSerializer eventSerializer)
    {
        if (connectionStringProvider is null)            
            throw new ArgumentNullException(nameof(connectionStringProvider));

        _dbConnString = connectionStringProvider.ConnectionString;
        _tableCreator = tableCreator ?? throw new ArgumentNullException(nameof(tableCreator));
        _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));           
    }

    protected override async Task PersistCoreAsync(TA aggregateRoot, CancellationToken cancellationToken = default)
    {
        await _tableCreator.EnsureTableAsync<TA, TKey>(cancellationToken)
                           .ConfigureAwait(false);

        using var dbConn = new SqlConnection(_dbConnString);
        await dbConn.OpenAsync().ConfigureAwait(false);

        using var transaction = dbConn.BeginTransaction();
        
        try
        {
            var lastVersion = await this.GetLastAggregateVersionAsync(aggregateRoot, dbConn, transaction)
                              .ConfigureAwait(false);
            if (lastVersion >= aggregateRoot.Version)
                throw new ArgumentOutOfRangeException(nameof(aggregateRoot), $"aggregate version mismatch, expected {aggregateRoot.Version}, got {lastVersion}");

            var tableName = _tableCreator.GetTableName<TA, TKey>();
            var sql = $@"INSERT INTO {tableName} (aggregateId, aggregateVersion, eventType, data, timestamp)
                         VALUES (@aggregateId, @aggregateVersion, @eventType, @data, @timestamp);";

            var entities = aggregateRoot.NewEvents.Select(evt => AggregateEvent.Create(evt, _eventSerializer))
                                               .ToList();
            await dbConn.ExecuteAsync(sql, param: entities, transaction: transaction)
                         .ConfigureAwait(false);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public override async Task<TA> RehydrateAsync(TKey key, CancellationToken cancellationToken = default)
    {
        await _tableCreator.EnsureTableAsync<TA, TKey>(cancellationToken)
                           .ConfigureAwait(false);

        var tableName = _tableCreator.GetTableName<TA, TKey>();
        var sql = $@"SELECT aggregateId, aggregateVersion, eventType, data, timestamp
                         FROM {tableName}
                         WHERE aggregateId = @aggregateId
                         ORDER BY aggregateVersion ASC";

        using var dbConn = new SqlConnection(_dbConnString);
        await dbConn.OpenAsync().ConfigureAwait(false);

        var aggregateEvents = await dbConn.QueryAsync<AggregateEvent>(sql, new { aggregateId = key })
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

    private async Task<long?> GetLastAggregateVersionAsync(TA aggregateRoot, SqlConnection dbConn, IDbTransaction transaction)
    {
        var tableName = _tableCreator.GetTableName<TA, TKey>();
        var sql = @$"SELECT TOP 1 aggregateVersion
                         FROM {tableName} 
                         WHERE aggregateId = @aggregateId
                         ORDER BY aggregateVersion DESC";
        var result = await dbConn.QueryFirstOrDefaultAsync<long?>(sql, param: new { aggregateId = aggregateRoot.Id }, transaction: transaction)
                                  .ConfigureAwait(false);
        return result;
    }
}