using Azure.Data.Tables;
using MediatR;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Common;
using SuperSafeBank.Common.EventBus;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.Events;
using SuperSafeBank.Service.Core.Azure.Common.Persistence;
using SuperSafeBank.Service.Core.Common.Queries;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSafeBank.Worker.Core.Azure.EventHandlers
{
    public class AccountEventsHandler :
        INotificationHandler<EventReceived<AccountCreated>>,
        INotificationHandler<EventReceived<Deposit>>,
        INotificationHandler<EventReceived<Withdrawal>>
    {
        private readonly IEventsRepository<Customer, Guid> _customersRepo;
        private readonly IEventsRepository<Account, Guid> _accountsRepo;
        private readonly IViewsContext _dbContext;
        private readonly ILogger<AccountEventsHandler> _logger;

        public AccountEventsHandler(
            IEventsRepository<Customer, Guid> customersRepo, 
            IEventsRepository<Account, Guid> accountsRepo, 
            IViewsContext dbContext, 
            ILogger<AccountEventsHandler> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customersRepo = customersRepo ?? throw new ArgumentNullException(nameof(customersRepo));
            _accountsRepo = accountsRepo ?? throw new ArgumentNullException(nameof(accountsRepo));
        }

        public async Task Handle(EventReceived<AccountCreated> @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("creating account details for aggregate {AggregateId} ...", @event.Event.AggregateId);

            var customerId = @event.Event.OwnerId;
            var customer = await _customersRepo.RehydrateAsync(customerId, cancellationToken);

            if (customer is null)
            {
                var msg = $"unable to find customer by id {@event.Event.OwnerId}";
                _logger.LogWarning(msg);
                throw new ArgumentOutOfRangeException(nameof(@event.Event.OwnerId), msg);
            }

            var balance = Money.Zero(@event.Event.Currency);
            var accountView = new AccountDetails(@event.Event.AggregateId,
                customer.Id, customer.Firstname, customer.Lastname, customer.Email.Value,
                balance);
            await UpsertAccountView(accountView, cancellationToken);

            _logger.LogInformation("created account {AggregateId}", @event.Event.AggregateId);
        }

        public async Task Handle(EventReceived<Deposit> @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("processing deposit of {Amount} on account {AggregateId} ...", @event.Event.Amount, @event.Event.AggregateId);

            var account = await _accountsRepo.RehydrateAsync(@event.Event.AggregateId, cancellationToken);
            var customer = await _customersRepo.RehydrateAsync(account.OwnerId, cancellationToken);

            var accountView = new AccountDetails(account.Id, 
                account.OwnerId, customer.Firstname, customer.Lastname, customer.Email.Value,
                account.Balance);

            await UpsertAccountView(accountView, cancellationToken);

            _logger.LogInformation("deposited {Amount} on account {AggregateId} ...", @event.Event.Amount, @event.Event.AggregateId);
        }

        public async Task Handle(EventReceived<Withdrawal> @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("processing withdrawal of {Amount} on account {AggregateId} ...", @event.Event.Amount, @event.Event.AggregateId);

            var account = await _accountsRepo.RehydrateAsync(@event.Event.AggregateId, cancellationToken);
            var customer = await _customersRepo.RehydrateAsync(account.OwnerId, cancellationToken);

            var accountView = new AccountDetails(account.Id,
                account.OwnerId, customer.Firstname, customer.Lastname, customer.Email.Value,
                account.Balance);

            await UpsertAccountView(accountView, cancellationToken);

            _logger.LogInformation("withdrawn {Amount} from account {AggregateId} ...", @event.Event.Amount, @event.Event.AggregateId);
        }

        private async Task UpsertAccountView(AccountDetails account, CancellationToken cancellationToken)
        {
            var entity = new ViewTableEntity()
            {
                PartitionKey = account.Id.ToString(),
                RowKey = account.Id.ToString(),
                Data = JsonSerializer.Serialize(account)
            };

            var response = await _dbContext.Accounts.UpsertEntityAsync(entity, mode: TableUpdateMode.Replace, cancellationToken: cancellationToken);
            if (response?.Status != 202)
            {
                var msg = $"an error has occurred while processing an event: {response.ReasonPhrase}";
                throw new Exception(msg);
            }
        }
    }
}