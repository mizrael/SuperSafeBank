using SuperSafeBank.Common;
using SuperSafeBank.Domain.Services;

namespace SuperSafeBank.Service.Core.Persistence.EventStore
{
    public class EventStoreCustomerEmailsService : ICustomerEmailsService
    {
        private readonly IAggregateRepository<CustomerEmail, string> _customerEmailRepository;

        public EventStoreCustomerEmailsService(IAggregateRepository<CustomerEmail, string> customerEmailRepository)
        {
            _customerEmailRepository = customerEmailRepository;
        }

        public Task CreateAsync(string email, Guid customerId, CancellationToken cancellationToken = default)
        => _customerEmailRepository.PersistAsync(new CustomerEmail(email, customerId), cancellationToken);

        public async Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            var aggregate = await _customerEmailRepository.RehydrateAsync(email, cancellationToken);
            return aggregate != null;
        }
    }
}