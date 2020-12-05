using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using SuperSafeBank.Web.Core.Queries.Models;

namespace SuperSafeBank.Web.Persistence.Mongo
{
    public class QueryDbContext : IQueryDbContext
    {
        private readonly IMongoDatabase _db;

        public QueryDbContext(IMongoDatabase db)
        {
            _db = db;

            if (!BsonClassMap.IsClassMapRegistered(typeof(AccountDetails)))
                BsonClassMap.RegisterClassMap<AccountDetails>(mapper=>
                {
                    mapper.MapProperty(c => c.Balance);
                    mapper.MapProperty(c => c.Id);
                    mapper.MapProperty(c => c.OwnerFirstName);
                    mapper.MapProperty(c => c.OwnerId);
                    mapper.MapProperty(c => c.OwnerLastName);
                    mapper.MapProperty(c => c.Version);
                    mapper.MapCreator(c => new AccountDetails(c.Id, c.OwnerId, c.OwnerFirstName, c.OwnerLastName, c.Balance, c.Version));
                });

            if (!BsonClassMap.IsClassMapRegistered(typeof(CustomerDetails)))
                BsonClassMap.RegisterClassMap<CustomerDetails>(mapper =>
            {
                mapper.MapProperty(c => c.Id);
                mapper.MapProperty(c => c.Firstname);
                mapper.MapProperty(c => c.Lastname);
                mapper.MapProperty(c => c.Email);
                mapper.MapProperty(c => c.Accounts);
                mapper.MapProperty(c => c.TotalBalance);
                mapper.MapCreator(c => new CustomerDetails(c.Id, c.Firstname, c.Lastname, c.Email, c.Accounts, c.TotalBalance));
            });

            if (!BsonClassMap.IsClassMapRegistered(typeof(CustomerArchiveItem)))
                BsonClassMap.RegisterClassMap<CustomerArchiveItem>(mapper =>
            {
                mapper.MapProperty(c => c.Id);
                mapper.MapProperty(c => c.Firstname);
                mapper.MapProperty(c => c.Lastname);
                mapper.MapProperty(c => c.Accounts);
                mapper.MapCreator(c => new CustomerArchiveItem(c.Id, c.Firstname, c.Lastname, c.Accounts));
            });

            AccountsDetails = _db.GetCollection<AccountDetails>("accounts");
            CustomersDetails = _db.GetCollection<CustomerDetails>("customerdetails");
            Customers = _db.GetCollection<CustomerArchiveItem>("customers");
        }

        public IMongoCollection<AccountDetails> AccountsDetails { get; }
        public IMongoCollection<CustomerDetails> CustomersDetails { get; }
        public IMongoCollection<CustomerArchiveItem> Customers { get; }
    }
}