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
            _logger.LogInformation($"creating customer details for aggregate {@event.Event.AggregateId} ...");

            var filter = Builders<CustomerDetails>.Filter
                .Eq(a => a.Id, @event.Event.AggregateId);

            var update = Builders<CustomerDetails>.Update
                .Set(a => a.Id, @event.Event.AggregateId)
                .Set(a => a.Version, @event.Event.AggregateVersion)
                .Set(a => a.Firstname, @event.Event.Firstname)
                .Set(a => a.Lastname, @event.Event.Lastname)
                .Set(a => a.TotalBalance, new Money(Currency.CanadianDollar, 0));

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

        public async Task Handle(EventReceived<Withdrawal> @event, CancellationToken cancellationToken)
        {
            var filter = Builders<AccountDetails>.Filter.Eq(a => a.Id, @event.Event.AggregateId);
            var account = await (await _db.AccountsDetails.FindAsync(filter, null, cancellationToken)).FirstOrDefaultAsync(cancellationToken);
            if (null == account)
            {
                var msg = $"unable to find account by id {@event.Event.AggregateId}";
                _logger.LogWarning(msg);
                throw new ArgumentOutOfRangeException(nameof(@event.Event.AggregateId), msg);
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
                var msg = $"unable to find account by id {@event.Event.AggregateId}";
                _logger.LogWarning(msg);
                throw new ArgumentOutOfRangeException(nameof(@event.Event.AggregateId), msg);
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