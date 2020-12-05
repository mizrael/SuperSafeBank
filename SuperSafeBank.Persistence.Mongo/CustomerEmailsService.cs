using MongoDB.Driver;
using SuperSafeBank.Domain.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SuperSafeBank.Persistence.Mongo
{
    public class CustomerEmailsService : ICustomerEmailsService
    {
        private readonly IMongoDatabase _db;
        private readonly IMongoCollection<CustomerEmail> _coll;
        
        public CustomerEmailsService(IMongoDatabase db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _coll = _db.GetCollection<CustomerEmail>("CustomerEmails");
        }

        public async Task CreateAsync(string email, Guid customerId)
        {
            var indexes = await (await _coll.Indexes.ListAsync()).ToListAsync();
            if (!indexes.Any())
            {
                var indexKeys = Builders<CustomerEmail>.IndexKeys.Ascending(a => a.Email);
                var createIndex = new CreateIndexModel<CustomerEmail>(indexKeys, new CreateIndexOptions()
                {
                    Unique = true,
                    Name = "email"
                });
                await _coll.Indexes.CreateOneAsync(createIndex);
            }

            var update = Builders<CustomerEmail>.Update
                .Set(a => a.Id, customerId)
                .Set(a => a.Email, email);

            await _coll.UpdateOneAsync(c => c.Email == email, update, options: new UpdateOptions() { IsUpsert = true });
        }

        public async Task<bool> ExistsAsync(string email)
        {
            var filter = Builders<CustomerEmail>.Filter
                .Eq(a => a.Email, email);

            var count = await _coll.CountDocumentsAsync(filter, new CountOptions()
            {
                Limit = 1
            });

            return count > 0;
        }
    }

    internal class CustomerEmail
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
    }
}
