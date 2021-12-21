using Azure.Data.Tables;

namespace SuperSafeBank.Service.Core.Azure.Common.Persistence
{
    public interface IViewsContext
    {
        TableClient Accounts { get; }
        TableClient CustomersArchive { get; }
        TableClient CustomersDetails { get; }
    }
}