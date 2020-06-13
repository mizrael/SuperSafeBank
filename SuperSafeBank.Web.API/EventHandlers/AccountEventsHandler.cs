using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.Events;
using SuperSafeBank.Web.API.Infrastructure;
using SuperSafeBank.Web.API.Queries.Models;
using SuperSafeBank.Web.API.Workers;

namespace SuperSafeBank.Web.API.EventHandlers
{
    public class AccountEventsHandler : 
        INotificationHandler<EventReceived<AccountCreated>>,
        INotificationHandler<EventReceived<Deposit>>,
        INotificationHandler<EventReceived<Withdrawal>>
    {
        private readonly IQueryDbContext _db;
        private readonly ILogger<AccountEventsHandler> _logger;

        public AccountEventsHandler(IQueryDbContext db, ILogger<AccountEventsHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task Handle(EventReceived<AccountCreated> @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("creating account details for aggregate {AggregateId} ...", @event.Event.AggregateId);

            var customerFilter = Builders<CustomerArchiveItem>.Filter
                .Eq(a => a.Id, @event.Event.OwnerId);

            var customer = await (await _db.Customers.FindAsync(customerFilter, null, cancellationToken))
                .FirstOrDefaultAsync(cancellationToken);
            if (null == customer) 
            {
                var msg = $"unable to find customer by id {@event.Event.OwnerId}";
                _logger.LogWarning(msg);
                throw new ArgumentOutOfRangeException(nameof(@event.Event.OwnerId), msg);
            }

            var filter = Builders<AccountDetails>.Filter
                .Eq(a => a.Id, @event.Event.AggregateId);

            var update = Builders<AccountDetails>.Update
                .Set(a => a.Id, @event.Event.AggregateId)
                .Set(a => a.Version, @event.Event.AggregateVersion)
                .Set(a => a.OwnerFirstName, customer.Firstname)
                .Set(a => a.OwnerLastName, customer.Lastname)
                .Set(a => a.OwnerId, @event.Event.OwnerId)
                .Set(a => a.Balance, new Money(@event.Event.Currency, 0));

            await _db.AccountsDetails.UpdateOneAsync(filter,
                cancellationToken: cancellationToken,
                update: update, 
                options: new UpdateOptions() { IsUpsert = true});

            _logger.LogInformation("created account {AggregateId}", @event.Event.AggregateId);
        }

        public async Task Handle(EventReceived<Deposit> @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("processing deposit of {@event.Event.Amount} on account {AggregateId} ...", @event.Event.AggregateId);

            var filter = Builders<AccountDetails>.Filter
                .And(Builders<AccountDetails>.Filter.Eq(a => a.Id, @event.Event.AggregateId),
                       Builders<AccountDetails>.Filter.Eq(a => a.Version, @event.Event.AggregateVersion-1));

            var update = Builders<AccountDetails>.Update
                .Set(a => a.Version, @event.Event.AggregateVersion)
                .Inc(a => a.Balance.Value, @event.Event.Amount.Value);
            var res = await _db.AccountsDetails.FindOneAndUpdateAsync(
                filter: filter,
                cancellationToken: cancellationToken,
                update: update,
                options: new FindOneAndUpdateOptions<AccountDetails, AccountDetails>() { IsUpsert = false });

            if(res != null) 
                _logger.LogInformation("deposited {@event.Event.Amount} on account {AggregateId} ...", @event.Event.AggregateId);
            else 
                _logger.LogWarning("deposit {@event.Event.Amount} on account {AggregateId} failed!", @event.Event.AggregateId);
        }

        public async Task Handle(EventReceived<Withdrawal> @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("processing withdrawal of {@event.Event.Amount} on account {AggregateId} ...", @event.Event.AggregateId);

            var filter = Builders<AccountDetails>.Filter
                .And(Builders<AccountDetails>.Filter.Eq(a => a.Id, @event.Event.AggregateId),
                    Builders<AccountDetails>.Filter.Eq(a => a.Version, @event.Event.AggregateVersion-1));

            var update = Builders<AccountDetails>.Update
                .Set(a => a.Version, @event.Event.AggregateVersion)
                .Inc(a => a.Balance.Value, -@event.Event.Amount.Value);
            var res = await _db.AccountsDetails.FindOneAndUpdateAsync(
                filter: filter,
                cancellationToken: cancellationToken,
                update: update,
                options: new FindOneAndUpdateOptions<AccountDetails, AccountDetails>() { IsUpsert = false });

            if (res != null)
                _logger.LogInformation("withdrawn {@event.Event.Amount} from account {AggregateId} ...", @event.Event.AggregateId);
            else 
                _logger.LogWarning("withdrawal of {@event.Event.Amount} from account {AggregateId} failed!", @event.Event.AggregateId);
        }
    }
}