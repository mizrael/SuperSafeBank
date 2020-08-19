using MongoDB.Driver;
using SuperSafeBank.Web.Core.Queries.Models;

namespace SuperSafeBank.Web.Persistence.Mongo
{
    public interface IQueryDbContext
    {
        IMongoCollection<AccountDetails> AccountsDetails { get; }
        IMongoCollection<CustomerDetails> CustomersDetails { get; }
        IMongoCollection<CustomerArchiveItem> Customers { get; }
    }
}