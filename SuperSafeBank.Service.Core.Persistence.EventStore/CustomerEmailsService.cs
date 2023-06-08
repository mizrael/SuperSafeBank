using SuperSafeBank.Common;
using SuperSafeBank.Domain.Services;

namespace SuperSafeBank.Service.Core.Persistence.EventStore
{
    public class CustomerEmailsService : ICustomerEmailsService
    {
        private readonly IAggregateRepository<CustomerEmail, string> _customerEmailRepository;

        public CustomerEmailsService(IAggregateRepository<CustomerEmail, string> customerEmailRepository)
        {
            _customerEmailRepository = customerEmailRepository;
        }

        public Task CreateAsync(string email, Guid customerId)
        => _customerEmailRepository.PersistAsync(new CustomerEmail(email, customerId));

        public async Task<bool> ExistsAsync(string email)
        {
            var aggregate = await _customerEmailRepository.RehydrateAsync(email);
            return aggregate != null;
        }
    }
}