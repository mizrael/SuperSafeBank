using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SuperSafeBank.Domain.Events;
using SuperSafeBank.Domain.Queries.Models;

namespace SuperSafeBank.Web.API.Workers
{
    public class CustomersArchiveHandler : 
        INotificationHandler<EventReceived<CustomerCreated>>,
        INotificationHandler<EventReceived<AccountCreated>>
    {
        private readonly IMongoDatabase _db;
        private readonly IMongoCollection<CustomerArchiveItem> _coll;
        private readonly ILogger<CustomersArchiveHandler> _logger;

        public CustomersArchiveHandler(IMongoDatabase db, ILogger<CustomersArchiveHandler> logger)
        {
            _db = db;
            _logger = logger;
            _coll = _db.GetCollection<CustomerArchiveItem>("customers");
        }

        public async Task Handle(EventReceived<CustomerCreated> @event, CancellationToken cancellationToken)
        {
            var filter = Builders<CustomerArchiveItem>.Filter
                .Eq(a => a.Id, @event.Event.AggregateId);

            var update = Builders<CustomerArchiveItem>.Update
                .Set(a => a.Id, @event.Event.AggregateId)
                .Set(a => a.Version, @event.Event.AggregateVersion)
                .Set(a => a.Firstname, @event.Event.Firstname)
                .Set(a => a.Lastname, @event.Event.Lastname);

            await _coll.UpdateOneAsync(filter,
                cancellationToken: cancellationToken,
                update: update,
                options: new UpdateOptions() { IsUpsert = true });

            _logger.LogInformation($"created customer archive item {@event.Event.AggregateId}");
        }

        public async Task Handle(EventReceived<AccountCreated> @event, CancellationToken cancellationToken)
        {
            var filter = Builders<CustomerArchiveItem>.Filter
                .Eq(a => a.Id, @event.Event.OwnerId);

            var update = Builders<CustomerArchiveItem>.Update
                .AddToSet(a => a.Accounts, @event.Event.AggregateId);

            await _coll.UpdateOneAsync(filter,
                cancellationToken: cancellationToken,
                update: update,
                options: new UpdateOptions() { IsUpsert = true });

            _logger.LogInformation($"updated customer archive item accounts {@event.Event.AggregateId}");
        }
    }
}