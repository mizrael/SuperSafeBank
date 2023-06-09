using SuperSafeBank.Common.Models;

namespace SuperSafeBank.Persistence.SQLServer
{
    public interface IAggregateTableCreator
    {
        Task EnsureTableAsync<TA, TKey>(CancellationToken cancellationToken = default) 
            where TA : class, IAggregateRoot<TKey>;

        string GetTableName<TA, TKey>()
            where TA : class, IAggregateRoot<TKey>;
    }
}