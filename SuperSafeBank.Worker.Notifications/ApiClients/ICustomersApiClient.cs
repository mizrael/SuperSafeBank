using System;
using System.Threading.Tasks;
using SuperSafeBank.Worker.Notifications.ApiClients.Models;

namespace SuperSafeBank.Worker.Notifications.ApiClients
{
    public interface ICustomersApiClient {
        Task<CustomerDetails> GetCustomerAsync(Guid customerId);
    }
}