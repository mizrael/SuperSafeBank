using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SuperSafeBank.Domain.Events;
using SuperSafeBank.Domain.Queries.Models;
using SuperSafeBank.Web.API.Infrastructure;

namespace SuperSafeBank.Web.API.Workers.EventHandlers
{
    public class CustomerDetailsHandler : 
        INotificationHandler<EventReceived<CustomerCreated>>,
        INotificationHandler<EventReceived<AccountCreated>>
    {
        private readonly IQueryDbContext _db;
        
        private readonly ILogger<CustomerDetailsHandler> _logger;

        public CustomerDetailsHandler(IQueryDbContext db, ILogger<CustomerDetailsHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task Handle(EventReceived<CustomerCreated> @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"creating customer details for aggregate {@event.Event.AggregateId} ...");

            var filter = Builders<CustomerDetails>.Filter
                .Eq(a => a.Id, @event.Event.AggregateId);

            var update = Builders<CustomerDetails>.Update
                .Set(a => a.Id, @event.Event.AggregateId)
                .Set(a => a.Version, @event.Event.AggregateVersion)
                .Set(a => a.Firstname, @event.Event.Firstname)
                .Set(a => a.Lastname, @event.Event.Lastname);

            await _db.CustomersDetails.UpdateOneAsync(filter,
                cancellationToken: cancellationToken,
                update: update,
                options: new UpdateOptions() { IsUpsert = true });

            _logger.LogInformation($"created customer details {@event.Event.AggregateId}");
        }

        public async Task Handle(EventReceived<AccountCreated> @event, CancellationToken cancellationToken)
        {
            var filter = Builders<CustomerDetails>.Filter
                .Eq(a => a.Id, @event.Event.OwnerId);

            var update = Builders<CustomerDetails>.Update
                .AddToSet(a => a.Accounts, @event.Event.AggregateId);

            await _db.CustomersDetails.UpdateOneAsync(filter,
                cancellationToken: cancellationToken,
                update: update,
                options: new UpdateOptions() { IsUpsert = true });

            _logger.LogInformation($"updated customer details accounts {@event.Event.AggregateId}");
        }
    }
}