using Azure.Data.Tables;
using MediatR;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Common.EventBus;
using SuperSafeBank.Domain.Events;
using SuperSafeBank.Service.Core.Azure.Common.Persistence;
using SuperSafeBank.Service.Core.Common.Queries;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSafeBank.Worker.Core.Azure.EventHandlers
{
    public class CustomersArchiveHandler : 
        INotificationHandler<EventReceived<CustomerCreated>>
    {
        private readonly ILogger<CustomersArchiveHandler> _logger;
        private readonly IViewsContext _dbContext;

        public CustomersArchiveHandler(IViewsContext dbContext, ILogger<CustomersArchiveHandler> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(EventReceived<CustomerCreated> @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("creating customer archive item for aggregate {AggregateId} ...", @event.Event.AggregateId);
                        
            var customerView = new CustomerArchiveItem(@event.Event.AggregateId, @event.Event.Firstname, @event.Event.Lastname);

            var entity = new ViewTableEntity()
            {
                PartitionKey = customerView.Id.ToString(),
                RowKey = customerView.Id.ToString(),
                Data = System.Text.Json.JsonSerializer.Serialize(customerView),
            };
            var response = await _dbContext.CustomersArchive.UpsertEntityAsync(entity, mode: TableUpdateMode.Replace, cancellationToken: cancellationToken);
            if (response?.Status != 202)
            {
                var msg = $"an error has occurred while processing an event: {response.ReasonPhrase}";
                throw new Exception(msg);
            }

            _logger.LogInformation($"created customer archive item {@event.Event.AggregateId}");
        }
    }
}