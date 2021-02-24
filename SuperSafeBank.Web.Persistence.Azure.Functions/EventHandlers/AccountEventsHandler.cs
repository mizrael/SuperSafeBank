using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Core.EventBus;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.Events;
using SuperSafeBank.Persistence.Azure;
using SuperSafeBank.Web.Core.Queries.Models;

namespace SuperSafeBank.Web.Persistence.Azure.Functions.EventHandlers
{
    public class AccountEventsHandler :
        INotificationHandler<EventReceived<AccountCreated>>,
        INotificationHandler<EventReceived<Deposit>>,
        INotificationHandler<EventReceived<Withdrawal>>
    {
        private readonly ILogger<AccountEventsHandler> _logger;
        private readonly Container _accountsContainer;
        private readonly Container _customerDetailsContainer;

        public AccountEventsHandler(IDbContainerProvider containerProvider, ILogger<AccountEventsHandler> logger)
        {
            if (containerProvider == null)
                throw new ArgumentNullException(nameof(containerProvider));

            _logger = logger;

            _accountsContainer = containerProvider.GetContainer("Accounts");
            _customerDetailsContainer = containerProvider.GetContainer("CustomersDetails");
        }

        public async Task Handle(EventReceived<AccountCreated> @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("creating account details for aggregate {AggregateId} ...", @event.Event.AggregateId);

            var customerId = @event.Event.OwnerId;
            var response = await _customerDetailsContainer.ReadItemAsync<CustomerDetails>(
                customerId.ToString(),
                new PartitionKey(customerId.ToString()),
                null, cancellationToken);

            var customer = response.Resource;
            if (null == customer)
            {
                var msg = $"unable to find customer by id {@event.Event.OwnerId}";
                _logger.LogWarning(msg);
                throw new ArgumentOutOfRangeException(nameof(@event.Event.OwnerId), msg);
            }

            var balance = Money.Zero(@event.Event.Currency);
            var newAccount = new AccountDetails(@event.Event.AggregateId,
                customer.Id, customer.Firstname, customer.Lastname,
                balance);
            await _accountsContainer.UpsertItemAsync(newAccount, new PartitionKey(@event.Event.AggregateId.ToString()),
                null, cancellationToken);

            _logger.LogInformation("created account {AggregateId}", @event.Event.AggregateId);
        }

        public async Task Handle(EventReceived<Deposit> @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("processing deposit of {Amount} on account {AggregateId} ...", @event.Event.Amount, @event.Event.AggregateId);

            var accountId = @event.Event.AggregateId;
            var partitionKey = new PartitionKey(accountId.ToString());
            var response = await _customerDetailsContainer.ReadItemAsync<AccountDetails>(
                accountId.ToString(),
                partitionKey,
                null, cancellationToken);

            var account = response.Resource;
            var newBalance = account.Balance.Add(@event.Event.Amount.Value);
            var updatedAccount = new AccountDetails(account.Id, 
                account.OwnerId, account.OwnerFirstName, account.OwnerLastName, 
                newBalance);
            
            await _accountsContainer.UpsertItemAsync(updatedAccount, partitionKey, null, cancellationToken);

            _logger.LogInformation("deposited {Amount} on account {AggregateId} ...", @event.Event.Amount, @event.Event.AggregateId);
        }

        public async Task Handle(EventReceived<Withdrawal> @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("processing withdrawal of {Amount} on account {AggregateId} ...", @event.Event.Amount, @event.Event.AggregateId);

            var accountId = @event.Event.AggregateId;
            var partitionKey = new PartitionKey(accountId.ToString());
            var response = await _customerDetailsContainer.ReadItemAsync<AccountDetails>(
                accountId.ToString(),
                partitionKey,
                null, cancellationToken);

            var account = response.Resource;
            var newBalance = account.Balance.Subtract(@event.Event.Amount.Value);
            var updatedAccount = new AccountDetails(account.Id,
                account.OwnerId, account.OwnerFirstName, account.OwnerLastName,
                newBalance);

            await _accountsContainer.UpsertItemAsync(updatedAccount, partitionKey, null, cancellationToken);

            _logger.LogInformation("withdrawn {Amount} from account {AggregateId} ...", @event.Event.Amount, @event.Event.AggregateId);
        }
    }
}