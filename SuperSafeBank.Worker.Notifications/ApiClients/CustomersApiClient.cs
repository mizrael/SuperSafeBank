using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Worker.Notifications.ApiClients.Models;

namespace SuperSafeBank.Worker.Notifications.ApiClients
{
    public class CustomersApiClient : ICustomersApiClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<CustomersApiClient> _logger;

        public CustomersApiClient(HttpClient client, ILogger<CustomersApiClient> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CustomerDetails> GetCustomerAsync(Guid customerId)
        {
            // TODO: as of now, System.Text.Json cannot deserialize immutable classes/structs
            var response = await _client.GetStringAsync($"customers/{customerId}");
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<CustomerDetails>(response);
            return result;
        } 
    }
}