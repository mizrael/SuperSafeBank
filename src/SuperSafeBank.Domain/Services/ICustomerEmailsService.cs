using System;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSafeBank.Domain.Services
{
    public interface ICustomerEmailsService
    {
        Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default);
        Task CreateAsync(string email, Guid customerId, CancellationToken cancellationToken = default);
    }
}