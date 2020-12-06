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
    public class AccountByIdHandler : IRequestHandler<AccountById, AccountDetails>
    {
        private readonly Container _container;

        public AccountByIdHandler(IDbContainerProvider containerProvider)
        {
            if (containerProvider == null)
                throw new ArgumentNullException(nameof(containerProvider));

            _container = containerProvider.GetContainer("AccountsDetails");
        }

        public async Task<AccountDetails> Handle(AccountById request, CancellationToken cancellationToken)
        {
            var partitionKey = new PartitionKey(request.Id.ToString());

            //by design, ReadItemAsync() throws is item not found

            var response = await _container.ReadItemStreamAsync(request.Id.ToString(), partitionKey, cancellationToken: cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            return _container.Database.Client.ClientOptions.Serializer.FromStream<AccountDetails>(response.Content);
        }
    }
}