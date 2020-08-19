using MongoDB.Bson;
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

            var collectionSettings = new MongoCollectionSettings
            {
                GuidRepresentation = GuidRepresentation.Standard
            };

            AccountsDetails = _db.GetCollection<AccountDetails>("accounts", collectionSettings);
            CustomersDetails = _db.GetCollection<CustomerDetails>("customerdetails", collectionSettings);
            Customers = _db.GetCollection<CustomerArchiveItem>("customers", collectionSettings);
        }

        public IMongoCollection<AccountDetails> AccountsDetails { get; }
        public IMongoCollection<CustomerDetails> CustomersDetails { get; }
        public IMongoCollection<CustomerArchiveItem> Customers { get; }
    }
}