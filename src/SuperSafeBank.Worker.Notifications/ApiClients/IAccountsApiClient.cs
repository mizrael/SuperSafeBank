using SuperSafeBank.Worker.Notifications.ApiClients.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSafeBank.Worker.Notifications.ApiClients
{
    public interface IAccountsApiClient
    {
        Task<AccountDetails> GetAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    }
}