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
    public class CustomerDetailsHandler :
        INotificationHandler<EventReceived<CustomerCreated>>,
        INotificationHandler<EventReceived<AccountCreated>>,
        INotificationHandler<EventReceived<Deposit>>,
        INotificationHandler<EventReceived<Withdrawal>>
    {
        private readonly ILogger<CustomerDetailsHandler> _logger;
        private readonly Container _container;

        public CustomerDetailsHandler(IDbContainerProvider containerProvider, ILogger<CustomerDetailsHandler> logger)
        {
            if (containerProvider == null)
                throw new ArgumentNullException(nameof(containerProvider));

            _logger = logger;

            _container = containerProvider.GetContainer("CustomersDetails");
        }

        public async Task Handle(EventReceived<CustomerCreated> @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("creating customer details for {AggregateId} ...", @event.Event.AggregateId);

            var partitionKey = new PartitionKey(@event.Event.AggregateId.ToString());

            var customer = new CustomerDetails(@event.Event.AggregateId, @event.Event.Firstname, @event.Event.Lastname, @event.Event.Email, null, new Money(Currency.CanadianDollar, 0));

            var response = await _container.UpsertItemAsync(customer, partitionKey, cancellationToken: cancellationToken);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                var msg = $"an error has occurred while processing an event: {response.Diagnostics}";
                throw new Exception(msg);
            }

            _logger.LogInformation("created customer details {AggregateId}", @event.Event.AggregateId);
        }

        public async Task Handle(EventReceived<AccountCreated> @event, CancellationToken cancellationToken)
        {
            var partitionKey = new PartitionKey(@event.Event.OwnerId.ToString());

            var response = await _container.ReadItemAsync<CustomerDetails>(@event.Event.OwnerId.ToString(),
                partitionKey,
                null, cancellationToken);

            var customer = response.Resource;

            var accounts = (customer.Accounts ?? Enumerable.Empty<Guid>()).ToList();
            accounts.Add(@event.Event.AggregateId);

            var updatedCustomer = new CustomerDetails(customer.Id, customer.Firstname, customer.Lastname, customer.Email, accounts, customer.TotalBalance );
            await _container.ReplaceItemAsync(updatedCustomer, @event.Event.OwnerId.ToString(), partitionKey, null, cancellationToken);

            _logger.LogInformation($"updated customer details accounts {@event.Event.AggregateId}");
        }

        public async Task Handle(EventReceived<Withdrawal> @event, CancellationToken cancellationToken)
        {
            var partitionKey = new PartitionKey(@event.Event.OwnerId.ToString());

            var response = await _container.ReadItemAsync<CustomerDetails>(@event.Event.OwnerId.ToString(),
                partitionKey,
                null, cancellationToken);

            var customer = response.Resource;
            var balance = (customer.TotalBalance ?? new Money(Currency.CanadianDollar, 0));
            var newBalance = balance.Subtract(@event.Event.Amount.Value);

            var updatedCustomer = new CustomerDetails(customer.Id, customer.Firstname, customer.Lastname, customer.Email, customer.Accounts, newBalance);
            await _container.ReplaceItemAsync(updatedCustomer, @event.Event.OwnerId.ToString(), partitionKey, null, cancellationToken);
        }

        public async Task Handle(EventReceived<Deposit> @event, CancellationToken cancellationToken)
        {
            var partitionKey = new PartitionKey(@event.Event.OwnerId.ToString());

            var response = await _container.ReadItemAsync<CustomerDetails>(@event.Event.OwnerId.ToString(),
                partitionKey,
                null, cancellationToken);

            var customer = response.Resource;
            var balance = (customer.TotalBalance ?? new Money(Currency.CanadianDollar, 0));
            var newBalance = balance.Add(@event.Event.Amount.Value);

            var updatedCustomer = new CustomerDetails(customer.Id, customer.Firstname, customer.Lastname, customer.Email, customer.Accounts, newBalance);
            await _container.ReplaceItemAsync(updatedCustomer, @event.Event.OwnerId.ToString(), partitionKey, null, cancellationToken);
        }
    }
}