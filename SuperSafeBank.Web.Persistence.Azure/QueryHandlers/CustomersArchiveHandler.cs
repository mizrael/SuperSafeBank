using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using SuperSafeBank.Persistence.Azure;
using SuperSafeBank.Web.Core.Queries;
using SuperSafeBank.Web.Core.Queries.Models;

namespace SuperSafeBank.Web.Persistence.Azure.QueryHandlers
{
    public class CustomersArchiveHandler : IRequestHandler<CustomersArchive, IEnumerable<CustomerArchiveItem>>
    {
        private readonly Container _container;

        public CustomersArchiveHandler(IDbContainerProvider containerProvider)
        {
            if (containerProvider == null)
                throw new ArgumentNullException(nameof(containerProvider));
            
            _container = containerProvider.GetContainer("CustomersArchive");
        }

        public async Task<IEnumerable<CustomerArchiveItem>> Handle(CustomersArchive request, CancellationToken cancellationToken)
        {
            var results = new List<CustomerArchiveItem>();

            var iterator = _container.GetItemLinqQueryable<CustomerArchiveItem>().ToFeedIterator();
            while (iterator.HasMoreResults)
            {
                foreach (var item in await iterator.ReadNextAsync(cancellationToken))
                {
                    results.Add(item);
                }
            }

            return results;
        }
    }
}