using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SuperSafeBank.Common;
using SuperSafeBank.Worker.Notifications.ApiClients.Models;

namespace SuperSafeBank.Worker.Notifications.ApiClients
{
    public class AccountsApiClient(HttpClient client) : IAccountsApiClient
    {
        private readonly HttpClient _client = client ?? throw new ArgumentNullException(nameof(client));

        public async Task<AccountDetails> GetAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
        {
            using var response = await _client.GetStreamAsync($"accounts/{accountId}", cancellationToken);
            var result = await JsonSerializer.DeserializeAsync<AccountDetails>(response, JsonSerializerDefaultOptions.Defaults, cancellationToken: cancellationToken);
            return result;
        }
    }
}