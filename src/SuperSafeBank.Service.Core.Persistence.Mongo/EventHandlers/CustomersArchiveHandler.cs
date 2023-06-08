using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SuperSafeBank.Common;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.IntegrationEvents;
using SuperSafeBank.Service.Core.Common.Queries;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace SuperSafeBank.Service.Core.Persistence.Mongo.EventHandlers
{
    public class CustomersArchiveHandler : 
        INotificationHandler<CustomerCreated>
    {
        private readonly IQueryDbContext _db;
        private readonly IAggregateRepository<Customer, Guid> _customersRepo; 
        private readonly ILogger<CustomersArchiveHandler> _logger;

        public CustomersArchiveHandler(
            IAggregateRepository<Customer, Guid> customersRepo,
            IQueryDbContext db, 
            ILogger<CustomersArchiveHandler> logger)
        {
            _customersRepo = customersRepo ?? throw new ArgumentNullException(nameof(customersRepo));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(CustomerCreated @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("creating customer archive item for customer {CustomerId} ...", @event.CustomerId);

            var customer = await _customersRepo.RehydrateAsync(@event.CustomerId, cancellationToken);

            var filter = Builders<CustomerArchiveItem>.Filter
                .Eq(a => a.Id, customer.Id);

            var update = Builders<CustomerArchiveItem>.Update
                .Set(a => a.Id, customer.Id)
                .Set(a => a.Firstname, customer.Firstname)
                .Set(a => a.Lastname, customer.Lastname);

            await _db.Customers.UpdateOneAsync(filter,
                cancellationToken: cancellationToken,
                update: update,
                options: new UpdateOptions() { IsUpsert = true });

            _logger.LogInformation($"created customer archive item for customer {@event.CustomerId}");
        }
    }
}