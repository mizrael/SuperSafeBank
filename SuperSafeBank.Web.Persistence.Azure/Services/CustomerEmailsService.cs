using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using SuperSafeBank.Domain.Services;
using SuperSafeBank.Persistence.Azure;

namespace SuperSafeBank.Web.Persistence.Azure.Services
{
    public class CustomerEmailsService : ICustomerEmailsService
    {
        private readonly Container _container;
        private const string ContainerName = "CustomerEmails";

        public CustomerEmailsService(IDbContainerProvider containerProvider)
        {
            if (containerProvider == null)
                throw new ArgumentNullException(nameof(containerProvider));

            _container = containerProvider.GetContainer(ContainerName);
        }

        public async Task<bool> ExistsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(email));
            var partitionKey = new PartitionKey(email);
            try
            {
                var item = await _container.ReadItemAsync<CustomerEmail>(email, partitionKey);
                return (null != item);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // https://github.com/Azure/azure-cosmos-dotnet-v3/issues/122#issuecomment-523865780
                return false;
            }
        }

        public async Task CreateAsync(string email, Guid customerId)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(email));

            var partitionKey = new PartitionKey(email);
            var item = new CustomerEmail()
                { Email = email, CustomerId = customerId};

            var response = await _container.CreateItemAsync(item, partitionKey);
            if(response.StatusCode != HttpStatusCode.Created)
                throw new Exception($"unable to create customer email '{email}'");
        }
    }

    internal class CustomerEmail
    {
        [JsonProperty(PropertyName = "id")]
        public string Email { get; set; }
        public Guid CustomerId { get; set; }
    }
}
