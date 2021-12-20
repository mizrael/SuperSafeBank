#if OnAzure
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using SuperSafeBank.Web.Core.Queries.Models;

namespace SuperSafeBank.Service.Core.Tests.Fixtures
{
    public class AzureQueryModelsSeeder : IQueryModelsSeeder
    {
        private readonly Database _db;

        public AzureQueryModelsSeeder(Database cosmosClient)
        {
            _db = cosmosClient;
        }

        public async Task CreateAccountDetails(AccountDetails model)
        {
            await _db.GetContainer("AccountsDetails").CreateItemAsync(model);
        }

        public async Task CreateCustomerArchiveItem(CustomerArchiveItem model)
        {
            await _db.GetContainer("CustomersArchive").CreateItemAsync(model);
        }

        public async Task CreateCustomerDetails(CustomerDetails model)
        {
            await _db.GetContainer("CustomersDetails").CreateItemAsync(model);
        }
    }
}

#endif
