#if OnPremise

using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using SuperSafeBank.Web.Persistence.Mongo;

namespace SuperSafeBank.Web.API.Tests.Fixtures
{
    public class OnPremiseQueryModelsSeeder : IQueryModelsSeeder
    {
        private readonly QueryDbContext _dbContext;

        public OnPremiseQueryModelsSeeder(string queryDbConnectionString, string queryDbName)
        {
            if (string.IsNullOrWhiteSpace(queryDbConnectionString))
                throw new ArgumentException($"'{nameof(queryDbConnectionString)}' cannot be null or whitespace", nameof(queryDbConnectionString));
            if (string.IsNullOrWhiteSpace(queryDbName))            
                throw new ArgumentException($"'{nameof(queryDbName)}' cannot be null or whitespace", nameof(queryDbName));
                 
            var client = new MongoClient(queryDbConnectionString);
            var db = client.GetDatabase(queryDbName);

            _dbContext = new QueryDbContext(db);
        }

        public async Task CreateCustomerDetails(Core.Queries.Models.CustomerDetails model)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));
            
            await _dbContext.CustomersDetails.InsertOneAsync(model);
        }

        public async Task CreateCustomerArchiveItem(Core.Queries.Models.CustomerArchiveItem model)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));

            await _dbContext.Customers.InsertOneAsync(model);
        }
    }
}

#endif
