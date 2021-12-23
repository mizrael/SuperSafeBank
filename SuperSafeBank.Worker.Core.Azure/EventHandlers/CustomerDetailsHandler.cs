using Azure.Data.Tables;
using MediatR;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Common;
using SuperSafeBank.Common.EventBus;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.Events;
using SuperSafeBank.Domain.Services;
using SuperSafeBank.Service.Core.Azure.Common.Persistence;
using SuperSafeBank.Service.Core.Common.Queries;
using System;
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
        private readonly ICurrencyConverter _currencyConverter;

        public CustomerDetailsHandler(
            IEventsRepository<Customer, Guid> customersRepo,
            IEventsRepository<Account, Guid> accountsRepo,
            IViewsContext dbContext,
            ICurrencyConverter currencyConverter,
            ILogger<CustomerDetailsHandler> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customersRepo = customersRepo ?? throw new ArgumentNullException(nameof(customersRepo));
            _accountsRepo = accountsRepo ?? throw new ArgumentNullException(nameof(accountsRepo));
            _currencyConverter = currencyConverter ?? throw new ArgumentNullException(nameof(currencyConverter));
        }

        public async Task Handle(EventReceived<CustomerCreated> @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("creating customer details for {AggregateId} ...", @event.Event.AggregateId);

            var customerView = await BuildCustomerViewAsync(@event.Event.AggregateId, cancellationToken);
            await SaveCustomerViewAsync(customerView, cancellationToken);

            _logger.LogInformation("created customer details {AggregateId}", @event.Event.AggregateId);
        }

        public async Task Handle(EventReceived<AccountCreated> @event, CancellationToken cancellationToken)
        {
            var customerView = await BuildCustomerViewAsync(@event.Event.OwnerId, cancellationToken);
            await SaveCustomerViewAsync(customerView, cancellationToken);

            _logger.LogInformation($"updated customer details accounts {@event.Event.AggregateId}");
        }

        public async Task Handle(EventReceived<Withdrawal> @event, CancellationToken cancellationToken)
        {
            var account = await _accountsRepo.RehydrateAsync(@event.Event.AggregateId, cancellationToken);

            var customerView = await BuildCustomerViewAsync(account.OwnerId, cancellationToken);
            await SaveCustomerViewAsync(customerView, cancellationToken);

            _logger.LogInformation($"updated customer balance {@event.Event.AggregateId}");
        }

        public async Task Handle(EventReceived<Deposit> @event, CancellationToken cancellationToken)
        {
            var account = await _accountsRepo.RehydrateAsync(@event.Event.AggregateId, cancellationToken);
            
            var customerView = await BuildCustomerViewAsync(account.OwnerId, cancellationToken);
            await SaveCustomerViewAsync(customerView, cancellationToken);

            _logger.LogInformation($"updated customer balance {@event.Event.AggregateId}");
        }

        private async Task<CustomerDetails> BuildCustomerViewAsync(Guid customerId, CancellationToken cancellationToken)
        {
            var customer = await _customersRepo.RehydrateAsync(customerId, cancellationToken);
            
            var totalBalance = Money.Zero(Currency.CanadianDollar);
            var accounts = new CustomerAccountDetails[customer.Accounts.Count];
            int index = 0;
            foreach(var id in customer.Accounts)
            {
                var account = await _accountsRepo.RehydrateAsync(id, cancellationToken);
                accounts[index++] = CustomerAccountDetails.Map(account);

                totalBalance = totalBalance.Add(account.Balance, _currencyConverter);
            }                

            var customerView = new CustomerDetails(customer.Id, customer.Firstname, customer.Lastname, customer.Email.Value, accounts, totalBalance);
            return customerView;
        }

        private async Task SaveCustomerViewAsync(CustomerDetails customerView, CancellationToken cancellationToken)
        {
            var entity = ViewTableEntity.Map(customerView);
            var response = await _dbContext.CustomersDetails.UpsertEntityAsync(entity, mode: TableUpdateMode.Replace, cancellationToken: cancellationToken);
            if (response?.Status != 204)
            {
                var msg = $"an error has occurred while processing an event: {response.ReasonPhrase}";
                throw new Exception(msg);
            }
        }


    }
}