using MediatR;
using SuperSafeBank.Service.Core.Azure.Common.Persistence;
using SuperSafeBank.Service.Core.Common.Queries;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSafeBank.Service.Core.Azure.QueryHandlers
{
    public class CustomerByIdHandler : IRequestHandler<CustomerById, CustomerDetails>
    {
        private readonly IViewsContext _dbContext;

        public CustomerByIdHandler(IViewsContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<CustomerDetails> Handle(CustomerById request, CancellationToken cancellationToken)
        {
            var response = await _dbContext.CustomersDetails.GetEntityAsync<ViewTableEntity>(
                partitionKey: request.CustomerId.ToString(),
                rowKey: string.Empty,
                cancellationToken: cancellationToken);

            if (response?.Value is null)
                return null;

            return System.Text.Json.JsonSerializer.Deserialize<CustomerDetails>(response.Value.Data);
        }
    }
}