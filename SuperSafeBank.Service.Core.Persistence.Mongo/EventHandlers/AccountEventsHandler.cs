using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SuperSafeBank.Common;
using SuperSafeBank.Common.EventBus;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.Events;
using SuperSafeBank.Domain.Services;
using SuperSafeBank.Service.Core.Common.Queries;

namespace SuperSafeBank.Service.Core.Persistence.Mongo.EventHandlers
{
    public class AccountEventsHandler : 
        INotificationHandler<EventReceived<AccountCreated>>,
        INotificationHandler<EventReceived<Deposit>>,
        INotificationHandler<EventReceived<Withdrawal>>
    {
        private readonly IQueryDbContext _db;
        private readonly IEventsRepository<Customer, Guid> _customersRepo;
        private readonly IEventsRepository<Account, Guid> _accountsRepo;
        private readonly ILogger<AccountEventsHandler> _logger;

        public AccountEventsHandler(
            IQueryDbContext db,
            IEventsRepository<Customer, Guid> customersRepo,
            IEventsRepository<Account, Guid> accountsRepo,
            ILogger<AccountEventsHandler> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _customersRepo = customersRepo ?? throw new ArgumentNullException(nameof(customersRepo));
            _accountsRepo = accountsRepo ?? throw new ArgumentNullException(nameof(accountsRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(EventReceived<AccountCreated> @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("creating account details for aggregate {AggregateId} ...", @event.Event.AggregateId);

            var accountView = await BuildAccountViewAsync(@event.Event.AggregateId, cancellationToken);
            await UpsertAccountViewAsync(accountView, cancellationToken);

            _logger.LogInformation("created account {AggregateId}", @event.Event.AggregateId);
        }

        public async Task Handle(EventReceived<Deposit> @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("processing deposit of {Amount} on account {AggregateId} ...", @event.Event.Amount, @event.Event.AggregateId);

            var accountView = await BuildAccountViewAsync(@event.Event.AggregateId, cancellationToken);
            await UpsertAccountViewAsync(accountView, cancellationToken);
            
            _logger.LogInformation("deposited {Amount} on account {AggregateId} ...", @event.Event.Amount, @event.Event.AggregateId);            
        }

        public async Task Handle(EventReceived<Withdrawal> @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("processing withdrawal of {Amount} on account {AggregateId} ...", @event.Event.Amount, @event.Event.AggregateId);

            var accountView = await BuildAccountViewAsync(@event.Event.AggregateId, cancellationToken);
            await UpsertAccountViewAsync(accountView, cancellationToken);

            _logger.LogInformation("withdrawn {Amount} from account {AggregateId} ...", @event.Event.Amount, @event.Event.AggregateId);
        }

        private async Task<AccountDetails> BuildAccountViewAsync(Guid accountId, CancellationToken cancellationToken)
        {
            var account = await _accountsRepo.RehydrateAsync(accountId, cancellationToken);
            var customer = await _customersRepo.RehydrateAsync(account.OwnerId, cancellationToken);

            var accountView = new AccountDetails(account.Id,
                account.OwnerId, customer.Firstname, customer.Lastname, customer.Email.Value,
                account.Balance);
            return accountView;
        }

        private async Task UpsertAccountViewAsync(AccountDetails accountView, CancellationToken cancellationToken)
        {
            var filter = Builders<AccountDetails>.Filter
                .Eq(a => a.Id, accountView.Id);

            var update = Builders<AccountDetails>.Update
                .Set(a => a.Id, accountView.Id)
                .Set(a => a.OwnerFirstName, accountView.OwnerFirstName)
                .Set(a => a.OwnerLastName, accountView.OwnerLastName)
                .Set(a => a.OwnerEmail, accountView.OwnerEmail)
                .Set(a => a.OwnerId, accountView.OwnerId)
                .Set(a => a.Balance, accountView.Balance);

            await _db.AccountsDetails.UpdateOneAsync(filter,
                cancellationToken: cancellationToken,
                update: update,
                options: new UpdateOptions() { IsUpsert = true });
        }

    }
}