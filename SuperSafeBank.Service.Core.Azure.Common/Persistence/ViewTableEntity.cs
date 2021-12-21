using Azure;
using Azure.Data.Tables;
using SuperSafeBank.Service.Core.Common.Queries;
using System.Text.Json;

namespace SuperSafeBank.Service.Core.Azure.Common.Persistence
{
    public record ViewTableEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string Data { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public static ViewTableEntity Map(CustomerDetails customerView)
            => new ViewTableEntity()
            {
                PartitionKey = customerView.Id.ToString(),
                RowKey = customerView.Id.ToString(),
                Data = JsonSerializer.Serialize(customerView),
            };
    }
}
