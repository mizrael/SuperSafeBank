using MongoDB.Driver;
using SuperSafeBank.Domain.Queries.Models;

namespace SuperSafeBank.Web.API.Infrastructure
{
    public interface IQueryDbContext
    {
        IMongoCollection<AccountDetails> AccountsDetails { get; }
        IMongoCollection<CustomerDetails> CustomersDetails { get; }
        IMongoCollection<CustomerArchiveItem> Customers { get; }
    }
}