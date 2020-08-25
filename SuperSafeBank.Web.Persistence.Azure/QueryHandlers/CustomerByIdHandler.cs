using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.Cosmos;
using SuperSafeBank.Persistence.Azure;
using SuperSafeBank.Web.Core.Queries;
using SuperSafeBank.Web.Core.Queries.Models;

namespace SuperSafeBank.Web.Persistence.Azure.QueryHandlers
{
    public class CustomerByIdHandler : IRequestHandler<CustomerById, CustomerDetails>
    {
        private readonly Container _container;

        public CustomerByIdHandler(IDbContainerProvider containerProvider)
        {
            if (containerProvider == null)
                throw new ArgumentNullException(nameof(containerProvider));

            _container = containerProvider.GetContainer("CustomersDetails");
        }

        public async Task<CustomerDetails> Handle(CustomerById request, CancellationToken cancellationToken)
        {
            var partitionKey = new PartitionKey(request.Id.ToString());

            //by design, ReadItemAsync() throws is item not found

            var response = await _container.ReadItemStreamAsync(request.Id.ToString(), partitionKey, cancellationToken: cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            return _container.Database.Client.ClientOptions.Serializer.FromStream<CustomerDetails>(response.Content);
        }
    }
}