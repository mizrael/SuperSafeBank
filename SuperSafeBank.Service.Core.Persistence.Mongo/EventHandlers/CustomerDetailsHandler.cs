using System;
using System.Collections.Generic;
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
    public class CustomerDetailsHandler :
        INotificationHandler<EventReceived<CustomerCreated>>,
        INotificationHandler<EventReceived<AccountCreated>>,
        INotificationHandler<EventReceived<Deposit>>,
        INotificationHandler<EventReceived<Withdrawal>>
    {
        private readonly IQueryDbContext _db;
        private readonly IEventsRepository<Customer, Guid> _customersRepo;
        private readonly IEventsRepository<Account, Guid> _accountsRepo;
        private readonly ICurrencyConverter _currencyConverter;
        private readonly ILogger<CustomerDetailsHandler> _logger;

        public CustomerDetailsHandler(
            IQueryDbContext db,
            IEventsRepository<Customer, Guid> customersRepo,
            IEventsRepository<Account, Guid> accountsRepo,
            ICurrencyConverter currencyConverter,
            ILogger<CustomerDetailsHandler> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _customersRepo = customersRepo ?? throw new ArgumentNullException(nameof(customersRepo));
            _accountsRepo = accountsRepo ?? throw new ArgumentNullException(nameof(accountsRepo));
            _currencyConverter = currencyConverter ?? throw new ArgumentNullException(nameof(currencyConverter));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            var account = await _accountsRepo.RehydrateAsync(@event.Event.AggregateId, cancellationToken);

            var customerView = await BuildCustomerViewAsync(account.OwnerId, cancellationToken);
            await SaveCustomerViewAsync(customerView, cancellationToken);

            _logger.LogInformation($"updated customer details accounts {@event.Event.AggregateId}");
        }

        public async Task Handle(EventReceived<Withdrawal> @event, CancellationToken cancellationToken)
        {
            var account = await _accountsRepo.RehydrateAsync(@event.Event.AggregateId, cancellationToken);

            var customerView = await BuildCustomerViewAsync(account.OwnerId, cancellationToken);
            await SaveCustomerViewAsync(customerView, cancellationToken);
        }

        public async Task Handle(EventReceived<Deposit> @event, CancellationToken cancellationToken)
        {
            var account = await _accountsRepo.RehydrateAsync(@event.Event.AggregateId, cancellationToken);

            var customerView = await BuildCustomerViewAsync(account.OwnerId, cancellationToken);
            await SaveCustomerViewAsync(customerView, cancellationToken);
        }

        private async Task<CustomerDetails> BuildCustomerViewAsync(Guid customerId, CancellationToken cancellationToken)
        {
            var customer = await _customersRepo.RehydrateAsync(customerId, cancellationToken);

            var totalBalance = Money.Zero(Currency.CanadianDollar);
            var accounts = new CustomerAccountDetails[customer.Accounts.Count];
            int index = 0;
            foreach (var id in customer.Accounts)
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
            var filter = Builders<CustomerDetails>.Filter
                            .Eq(a => a.Id, customerView.Id);

            var update = Builders<CustomerDetails>.Update
                .Set(a => a.Id, customerView.Id)
                .Set(a => a.Firstname, customerView.Firstname)
                .Set(a => a.Lastname, customerView.Lastname)
                .Set(a => a.Email, customerView.Email)
                .Set(a => a.Accounts, customerView.Accounts)
                .Set(a => a.TotalBalance, customerView.TotalBalance);

            await _db.CustomersDetails.UpdateOneAsync(filter,
                cancellationToken: cancellationToken,
                update: update,
                options: new UpdateOptions() { IsUpsert = true });
        }

    }
}