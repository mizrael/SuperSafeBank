using Azure.Data.Tables;

namespace SuperSafeBank.Service.Core.Azure.Common.Persistence
{
    public class ViewsContext : IViewsContext
    {
        public ViewsContext(string connectionString, string tablesPrefix)
        {
            this.CustomersDetails = new TableClient(connectionString, $"{tablesPrefix}{nameof(this.CustomersDetails)}");
            this.CustomersDetails.CreateIfNotExists();

            this.CustomersArchive = new TableClient(connectionString, $"{tablesPrefix}{nameof(this.CustomersArchive)}");
            this.CustomersArchive.CreateIfNotExists();

            this.Accounts = new TableClient(connectionString, $"{tablesPrefix}{nameof(this.Accounts)}");
            this.Accounts.CreateIfNotExists();
        }

        public TableClient CustomersDetails { get; }
        public TableClient CustomersArchive { get; }
        public TableClient Accounts { get; }
    }
}
