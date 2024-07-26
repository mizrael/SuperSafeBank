using Azure;
using Azure.Data.Tables;
using SuperSafeBank.Domain.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSafeBank.Service.Core.Azure.Services
{
    public class CustomerEmailsService(TableClient client) : ICustomerEmailsService
    {
        private readonly TableClient _client = client;

        public async Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(email));

            var results = _client.QueryAsync<CustomerEmail>(ce => ce.PartitionKey == email, cancellationToken: cancellationToken).ConfigureAwait(false);

            await foreach(var result in results)
            {
                return true;
            }

            return false;
        }

        public async Task CreateAsync(string email, Guid customerId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(email));

            var item = CustomerEmail.Create(email, customerId);
            await _client.AddEntityAsync(item, cancellationToken);
        }
    }

    internal record CustomerEmail : ITableEntity
    {
        public string Email => this.PartitionKey;
        public string CustomerId => this.RowKey;

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public static CustomerEmail Create(string email, Guid customerId)
            => new CustomerEmail()
            {
                PartitionKey = email,
                RowKey = customerId.ToString()
            };
    }
}
