using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SuperSafeBank.Common.EventBus;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.Events;
using SuperSafeBank.Service.Core.Common.Queries;

namespace SuperSafeBank.Service.Core.Persistence.Mongo.EventHandlers
{
    public class CustomerDetailsHandler :
        INotificationHandler<EventReceived<CustomerCreated>>,
        INotificationHandler<EventReceived<AccountCreated>>,
        INotificationHandler<EventReceived<Deposit>>,
        INotificationHandler<EventReceived<Withdrawal>>
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
            _logger.LogInformation("creating customer details for {AggregateId} ...", @event.Event.AggregateId);

            var filter = Builders<CustomerDetails>.Filter
                .Eq(a => a.Id, @event.Event.AggregateId);

            var update = Builders<CustomerDetails>.Update
                .Set(a => a.Id, @event.Event.AggregateId)
                .Set(a => a.Firstname, @event.Event.Firstname)
                .Set(a => a.Lastname, @event.Event.Lastname)
                .Set(a => a.Email, @event.Event.Email.Value)
                .Set(a => a.Accounts, new System.Guid[] { })
                .Set(a => a.TotalBalance, new Money(Currency.CanadianDollar, 0));

            await _db.CustomersDetails.UpdateOneAsync(filter,
                cancellationToken: cancellationToken,
                update: update,
                options: new UpdateOptions() { IsUpsert = true });

            _logger.LogInformation("created customer details {AggregateId}", @event.Event.AggregateId);
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

        public async Task Handle(EventReceived<Withdrawal> @event, CancellationToken cancellationToken)
        {
            var filter = Builders<AccountDetails>.Filter.Eq(a => a.Id, @event.Event.AggregateId);
            var account = await (await _db.AccountsDetails.FindAsync(filter, null, cancellationToken)).FirstOrDefaultAsync(cancellationToken);
            if (null == account)
            {
                _logger.LogWarning("unable to find account by id {AggregateId}", @event.Event.AggregateId);
                throw new ArgumentOutOfRangeException(nameof(@event.Event.AggregateId), $"unable to find account by id {@event.Event.AggregateId}" );
            }

            var customerFilter = Builders<CustomerDetails>.Filter.Eq(a => a.Id, account.OwnerId);
            var update = Builders<CustomerDetails>.Update
                .Inc(a => a.TotalBalance.Value, -@event.Event.Amount.Value);

            var res = await _db.CustomersDetails.FindOneAndUpdateAsync(customerFilter, update, null, cancellationToken);
            if (null == res)
            {
                var msg = $"unable to find customer by id {account.OwnerId}";
                _logger.LogWarning(msg);
                throw new ArgumentOutOfRangeException(nameof(account.OwnerId), msg);
            }
        }

        public async Task Handle(EventReceived<Deposit> @event, CancellationToken cancellationToken)
        {
            var filter = Builders<AccountDetails>.Filter.Eq(a => a.Id, @event.Event.AggregateId);
            var account = await (await _db.AccountsDetails.FindAsync(filter, null, cancellationToken)).FirstOrDefaultAsync(cancellationToken);
            if (null == account)
            {
                _logger.LogWarning("unable to find account by id {AggregateId}", @event.Event.AggregateId);
                throw new ArgumentOutOfRangeException(nameof(@event.Event.AggregateId), $"unable to find account by id {@event.Event.AggregateId}");
            }

            var customerFilter = Builders<CustomerDetails>.Filter.Eq(a => a.Id, account.OwnerId);
            var update = Builders<CustomerDetails>.Update
                .Inc(a => a.TotalBalance.Value, @event.Event.Amount.Value);

            var res = await _db.CustomersDetails.FindOneAndUpdateAsync(customerFilter, update, null, cancellationToken);
            if (null == res)
            {
                var msg = $"unable to find customer by id {account.OwnerId}";
                _logger.LogWarning(msg);
                throw new ArgumentOutOfRangeException(nameof(account.OwnerId), msg);
            }
        }
    }
}