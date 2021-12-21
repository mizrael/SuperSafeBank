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
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSafeBank.Worker.Core.Azure.EventHandlers
{
    public class CustomerDetailsHandler :
        INotificationHandler<EventReceived<CustomerCreated>>,
        INotificationHandler<EventReceived<AccountCreated>>,
        INotificationHandler<EventReceived<Deposit>>,
        INotificationHandler<EventReceived<Withdrawal>>
    {
        private readonly IViewsContext _dbContext;
        private readonly ILogger<CustomerDetailsHandler> _logger;
        private readonly IEventsRepository<Customer, Guid> _customersRepo;
        private readonly IEventsRepository<Account, Guid> _accountsRepo;

        public CustomerDetailsHandler(
            IEventsRepository<Customer, Guid> customersRepo,
            IEventsRepository<Account, Guid> accountsRepo,
            IViewsContext dbContext,
            ILogger<CustomerDetailsHandler> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customersRepo = customersRepo ?? throw new ArgumentNullException(nameof(customersRepo));
            _accountsRepo = accountsRepo ?? throw new ArgumentNullException(nameof(accountsRepo));
        }

        public async Task Handle(EventReceived<CustomerCreated> @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("creating customer details for {AggregateId} ...", @event.Event.AggregateId);

            var customerView = new CustomerDetails(
                @event.Event.AggregateId,
                @event.Event.Firstname,
                @event.Event.Lastname,
                @event.Event.Email.Value,
                null,
                new Money(Currency.CanadianDollar, 0));

            await SaveCustomerViewAsync(customerView, cancellationToken);

            _logger.LogInformation("created customer details {AggregateId}", @event.Event.AggregateId);
        }
        public async Task Handle(EventReceived<AccountCreated> @event, CancellationToken cancellationToken)
        {
            var response = await _dbContext.CustomersDetails.GetEntityAsync<ViewTableEntity>(
                partitionKey: @event.Event.OwnerId.ToString(),
                rowKey: string.Empty,
                cancellationToken: cancellationToken);
            var entity = response.Value;

            var customer = JsonSerializer.Deserialize<CustomerDetails>(entity.Data);

            var accounts = (customer.Accounts ?? Enumerable.Empty<Guid>()).ToList();
            accounts.Add(@event.Event.AggregateId);

            var customerView = new CustomerDetails(customer.Id, customer.Firstname, customer.Lastname, customer.Email, accounts, customer.TotalBalance );
            await SaveCustomerViewAsync(customerView, cancellationToken);

            _logger.LogInformation($"updated customer details accounts {@event.Event.AggregateId}");
        }

        public async Task Handle(EventReceived<Withdrawal> @event, CancellationToken cancellationToken)
        {
            var account = await _accountsRepo.RehydrateAsync(@event.Event.AggregateId, cancellationToken);
            var customer = await _customersRepo.RehydrateAsync(account.OwnerId, cancellationToken);
            
            var customerView = new CustomerDetails(customer.Id, customer.Firstname, customer.Lastname, customer.Email.Value, customer.Accounts, account.Balance);
            await SaveCustomerViewAsync(customerView, cancellationToken);

            _logger.LogInformation($"updated customer balance {@event.Event.AggregateId}");
        }

        public async Task Handle(EventReceived<Deposit> @event, CancellationToken cancellationToken)
        {
            var account = await _accountsRepo.RehydrateAsync(@event.Event.AggregateId, cancellationToken);
            var customer = await _customersRepo.RehydrateAsync(account.OwnerId, cancellationToken);

            var customerView = new CustomerDetails(customer.Id, customer.Firstname, customer.Lastname, customer.Email.Value, customer.Accounts, account.Balance);
            await SaveCustomerViewAsync(customerView, cancellationToken);

            _logger.LogInformation($"updated customer balance {@event.Event.AggregateId}");
        }

        private async Task SaveCustomerViewAsync(CustomerDetails customerView, CancellationToken cancellationToken)
        {
            var entity = new ViewTableEntity()
            {
                PartitionKey = customerView.Id.ToString(),
                RowKey = customerView.Id.ToString(),
                Data = JsonSerializer.Serialize(customerView),
            };
            var response = await _dbContext.CustomersDetails.UpsertEntityAsync(entity, mode: TableUpdateMode.Replace, cancellationToken: cancellationToken);
            if (response?.Status != 202)
            {
                var msg = $"an error has occurred while processing an event: {response.ReasonPhrase}";
                throw new Exception(msg);
            }
        }


    }
}