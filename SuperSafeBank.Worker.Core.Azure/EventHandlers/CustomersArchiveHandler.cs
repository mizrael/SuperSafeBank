using Azure.Data.Tables;
using MediatR;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Common;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.IntegrationEvents;
using SuperSafeBank.Service.Core.Azure.Common.Persistence;
using SuperSafeBank.Service.Core.Common.Queries;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSafeBank.Worker.Core.Azure.EventHandlers
{
    public class CustomersArchiveHandler : 
        INotificationHandler<CustomerCreated>
    {
        private readonly ILogger<CustomersArchiveHandler> _logger;
        private readonly IViewsContext _dbContext;
        private readonly IAggregateRepository<Customer, Guid> _customersRepo;

        public CustomersArchiveHandler(IViewsContext dbContext, IAggregateRepository<Customer, Guid> customersRepo, ILogger<CustomersArchiveHandler> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customersRepo = customersRepo ?? throw new ArgumentNullException(nameof(customersRepo));
        }

        public async Task Handle(CustomerCreated @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("creating customer archive item for aggregate {AggregateId} ...", @event.CustomerId);

            var customer = await _customersRepo.RehydrateAsync(@event.CustomerId, cancellationToken);

            var customerView = new CustomerArchiveItem(customer.Id, customer.Firstname, customer.Lastname);

            var entity = ViewTableEntity.Map(customerView);
            var response = await _dbContext.CustomersArchive.UpsertEntityAsync(entity, mode: TableUpdateMode.Replace, cancellationToken: cancellationToken);
            if (response?.Status != 204)
            {
                var msg = $"an error has occurred while processing an event: {response.ReasonPhrase}";
                throw new Exception(msg);
            }

            _logger.LogInformation($"created customer archive item {@event.CustomerId}");
        }
    }
}