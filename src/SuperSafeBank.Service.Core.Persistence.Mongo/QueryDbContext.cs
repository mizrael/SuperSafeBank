using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using SuperSafeBank.Service.Core.Common.Queries;

namespace SuperSafeBank.Service.Core.Persistence.Mongo
{
    public class QueryDbContext : IQueryDbContext
    {
        private readonly IMongoDatabase _db;

        private static readonly IBsonSerializer guidSerializer = new GuidSerializer(GuidRepresentation.Standard);
        private static readonly IBsonSerializer<decimal> decimalSerializer = new DecimalSerializer(BsonType.Decimal128);

        static QueryDbContext()
        {
            RegisterMappings();
        }

        public QueryDbContext(IMongoDatabase db)
        {
            _db = db ?? throw new System.ArgumentNullException(nameof(db));            

            AccountsDetails = _db.GetCollection<AccountDetails>("accounts");
            CustomersDetails = _db.GetCollection<CustomerDetails>("customerdetails");
            Customers = _db.GetCollection<CustomerArchiveItem>("customers");
        }

        private static void RegisterMappings()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(Domain.Currency)))
                BsonClassMap.RegisterClassMap<Domain.Currency>(mapper =>
                {
                    mapper.MapProperty(c => c.Name);
                    mapper.MapProperty(c => c.Symbol);
                    mapper.MapCreator(c => new Domain.Currency(c.Name, c.Symbol));
                });

            if (!BsonClassMap.IsClassMapRegistered(typeof(Domain.Money)))
                BsonClassMap.RegisterClassMap<Domain.Money>(mapper =>
                {
                    mapper.MapProperty(c => c.Currency);
                    mapper.MapProperty(c => c.Value).SetSerializer(decimalSerializer);
                    mapper.MapCreator(c => new Domain.Money(c.Currency, c.Value));
                });

            if (!BsonClassMap.IsClassMapRegistered(typeof(AccountDetails)))
                BsonClassMap.RegisterClassMap<AccountDetails>(mapper =>
                {
                    mapper.MapIdProperty(c => c.Id).SetSerializer(guidSerializer);
                    mapper.MapProperty(c => c.Balance);
                    mapper.MapProperty(c => c.OwnerFirstName);
                    mapper.MapProperty(c => c.OwnerId);
                    mapper.MapProperty(c => c.OwnerLastName);
                    mapper.MapProperty(c => c.OwnerEmail);
                    mapper.MapCreator(c => new AccountDetails(c.Id, c.OwnerId, c.OwnerFirstName, c.OwnerLastName, c.OwnerEmail, c.Balance));
                });

            if (!BsonClassMap.IsClassMapRegistered(typeof(CustomerDetails)))
                BsonClassMap.RegisterClassMap<CustomerDetails>(mapper =>
                {
                    mapper.MapIdProperty(c => c.Id).SetSerializer(guidSerializer);
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
                    mapper.MapIdProperty(c => c.Id).SetSerializer(guidSerializer);
                    mapper.MapProperty(c => c.Firstname);
                    mapper.MapProperty(c => c.Lastname);                    
                    mapper.MapCreator(c => new CustomerArchiveItem(c.Id, c.Firstname, c.Lastname));
                });
        }

        public IMongoCollection<AccountDetails> AccountsDetails { get; }
        public IMongoCollection<CustomerDetails> CustomersDetails { get; }
        public IMongoCollection<CustomerArchiveItem> Customers { get; }
    }
}