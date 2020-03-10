using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.Events;

namespace SuperSafeBank.Console
{
    public class AccountEventsHandler : 
        INotificationHandler<EventReceived<AccountCreated>>,
        INotificationHandler<EventReceived<Deposit>>,
        INotificationHandler<EventReceived<Withdrawal>>
    {
        private readonly IMongoDatabase _db;
        private readonly IMongoCollection<AccountView> _coll;
        private readonly ILogger<AccountEventsHandler> _logger;

        public AccountEventsHandler(IMongoDatabase db, ILogger<AccountEventsHandler> logger)
        {
            _db = db;
            _logger = logger;
            _coll = _db.GetCollection<AccountView>("accounts");
        }

        public async Task Handle(EventReceived<AccountCreated> @event, CancellationToken cancellationToken)
        {
            var filter = Builders<AccountView>.Filter
                .Eq(a => a.Id, @event.Event.AggregateId);

            var update = Builders<AccountView>.Update
                .Set(a => a.Id, @event.Event.AggregateId)
                .Set(a => a.Version, @event.Event.AggregateVersion)
                .Set(a => a.OwnerId, @event.Event.OwnerId)
                .Set(a => a.Balance, new Money(@event.Event.Currency, 0));

            await _coll.UpdateOneAsync(filter,
                cancellationToken: cancellationToken,
                update: update, 
                options: new UpdateOptions() { IsUpsert = true});

            _logger.LogInformation($"created account {@event.Event.AggregateId}");
        }

        public async Task Handle(EventReceived<Deposit> @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"processing deposit of {@event.Event.Amount} on account {@event.Event.AggregateId} ...");

            var filter = Builders<AccountView>.Filter
                .And(Builders<AccountView>.Filter.Eq(a => a.Id, @event.Event.AggregateId),
                       Builders<AccountView>.Filter.Eq(a => a.Version, @event.Event.AggregateVersion-1));

            var update = Builders<AccountView>.Update
                .Set(a => a.Version, @event.Event.AggregateVersion)
                .Inc(a => a.Balance.Value, @event.Event.Amount.Value);
            var res = await _coll.FindOneAndUpdateAsync(
                filter: filter,
                cancellationToken: cancellationToken,
                update: update,
                options: new FindOneAndUpdateOptions<AccountView, AccountView>() { IsUpsert = false });

            if(res != null) 
                _logger.LogInformation($"deposited {@event.Event.Amount} on account {@event.Event.AggregateId}");
            else 
                _logger.LogWarning($"deposit {@event.Event.Amount} on account {@event.Event.AggregateId} failed!");
        }

        public async Task Handle(EventReceived<Withdrawal> @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"processing withdrawal of {@event.Event.Amount} on account {@event.Event.AggregateId} ...");

            var filter = Builders<AccountView>.Filter
                .And(Builders<AccountView>.Filter.Eq(a => a.Id, @event.Event.AggregateId),
                    Builders<AccountView>.Filter.Eq(a => a.Version, @event.Event.AggregateVersion-1));

            var update = Builders<AccountView>.Update
                .Set(a => a.Version, @event.Event.AggregateVersion)
                .Inc(a => a.Balance.Value, -@event.Event.Amount.Value);
            var res = await _coll.FindOneAndUpdateAsync(
                filter: filter,
                cancellationToken: cancellationToken,
                update: update,
                options: new FindOneAndUpdateOptions<AccountView, AccountView>() { IsUpsert = false });

            if (res != null)
                _logger.LogInformation($"withdrawn {@event.Event.Amount} from account {@event.Event.AggregateId}");
            else 
                _logger.LogWarning($"withdrawal of {@event.Event.Amount} from account {@event.Event.AggregateId} failed!");
        }
    }
}