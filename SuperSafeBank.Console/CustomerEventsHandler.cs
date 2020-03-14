using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SuperSafeBank.Domain.Events;
using SuperSafeBank.Domain.Queries.Models;

namespace SuperSafeBank.Console
{
    public class CustomerEventsHandler : INotificationHandler<EventReceived<CustomerCreated>>
    {
        private readonly IMongoDatabase _db;
        private readonly IMongoCollection<CustomerDetails> _coll;
        private readonly ILogger<CustomerEventsHandler> _logger;

        public CustomerEventsHandler(IMongoDatabase db, ILogger<CustomerEventsHandler> logger)
        {
            _db = db;
            _logger = logger;
            _coll = _db.GetCollection<CustomerDetails>("customerdetails");
        }

        public async Task Handle(EventReceived<CustomerCreated> @event, CancellationToken cancellationToken)
        {
            var filter = Builders<CustomerDetails>.Filter
                .Eq(a => a.Id, @event.Event.AggregateId);

            var update = Builders<CustomerDetails>.Update
                .Set(a => a.Id, @event.Event.AggregateId)
                .Set(a => a.Version, @event.Event.AggregateVersion)
                .Set(a => a.Firstname, @event.Event.Firstname)
                .Set(a => a.Lastname, @event.Event.Lastname);

            await _coll.UpdateOneAsync(filter,
                cancellationToken: cancellationToken,
                update: update,
                options: new UpdateOptions() { IsUpsert = true });

            _logger.LogInformation($"created customer {@event.Event.AggregateId}");
        }
    }
}