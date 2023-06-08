using MongoDB.Driver;
using SuperSafeBank.Service.Core.Common.Queries;

namespace SuperSafeBank.Service.Core.Persistence.Mongo
{
    public interface IQueryDbContext
    {
        IMongoCollection<AccountDetails> AccountsDetails { get; }
        IMongoCollection<CustomerDetails> CustomersDetails { get; }
        IMongoCollection<CustomerArchiveItem> Customers { get; }
    }
}