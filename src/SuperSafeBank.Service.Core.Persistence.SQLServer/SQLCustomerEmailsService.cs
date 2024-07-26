using Microsoft.EntityFrameworkCore;
using SuperSafeBank.Domain.Services;

namespace SuperSafeBank.Service.Core.Persistence.SQLServer
{
    public class SQLCustomerEmailsService(CustomerDbContext dbContext) : ICustomerEmailsService
    {
        private readonly CustomerDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task CreateAsync(string email, Guid customerId, CancellationToken cancellationToken = default)
        {
            await _dbContext.CustomerEmails.AddAsync(new CustomerEmail(customerId, email), cancellationToken)
                                           .ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken)
                            .ConfigureAwait(false);
        }

        public Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default)
        => _dbContext.CustomerEmails.AnyAsync(e => e.Email == email, cancellationToken);
    }
}