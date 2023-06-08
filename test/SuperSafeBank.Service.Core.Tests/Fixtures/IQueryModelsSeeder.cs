using System.Threading.Tasks;

namespace SuperSafeBank.Service.Core.Tests.Fixtures
{
    public interface IQueryModelsSeeder
    {
        Task CreateCustomerDetails(Common.Queries.CustomerDetails model);
        Task CreateCustomerArchiveItem(Common.Queries.CustomerArchiveItem model);
        Task CreateAccountDetails(Common.Queries.AccountDetails model);
    }
}