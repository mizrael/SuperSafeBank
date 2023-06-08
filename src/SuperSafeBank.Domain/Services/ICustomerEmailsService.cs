using System;
using System.Threading.Tasks;

namespace SuperSafeBank.Domain.Services
{
    public interface ICustomerEmailsService
    {
        Task<bool> ExistsAsync(string email);
        Task CreateAsync(string email, Guid customerId);
    }
}