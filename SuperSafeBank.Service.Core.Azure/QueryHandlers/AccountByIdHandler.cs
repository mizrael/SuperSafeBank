using MediatR;
using SuperSafeBank.Service.Core.Azure.Common.Persistence;
using SuperSafeBank.Service.Core.Common.Queries;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSafeBank.Service.Core.Azure.QueryHandlers
{
    public class AccountByIdHandler : IRequestHandler<AccountById, AccountDetails>
    {
        private readonly IViewsContext _dbContext;

        public AccountByIdHandler(IViewsContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<AccountDetails> Handle(AccountById request, CancellationToken cancellationToken)
        {
            var response = await _dbContext.CustomersDetails.GetEntityAsync<ViewTableEntity>(
                partitionKey: request.AccountId.ToString(),
                rowKey: string.Empty,
                cancellationToken: cancellationToken);

            if (response?.Value is null)
                return null;

            return System.Text.Json.JsonSerializer.Deserialize<AccountDetails>(response.Value.Data);
        }
    }
}