using Azure;
using Azure.Data.Tables;

namespace SuperSafeBank.Service.Core.Azure.Common.Persistence
{
    public record ViewTableEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string Data { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
