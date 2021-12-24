using System;
using System.Threading.Tasks;

namespace SuperSafeBank.Worker.Notifications
{
    public interface INotificationsFactory
    {
        Task<Notification> CreateNewAccountNotificationAsync(Guid accountId);
        Task<Notification> CreateTransactionNotificationAsync(Guid accountId);
    }
}