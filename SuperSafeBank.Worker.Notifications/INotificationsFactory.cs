using System;
using System.Threading.Tasks;
using SuperSafeBank.Domain;

namespace SuperSafeBank.Worker.Notifications
{
    public interface INotificationsFactory
    {
        Task<Notification> CreateNewAccountNotificationAsync(Guid customerId, Guid accountId);
        Task<Notification> CreateDepositNotificationAsync(Guid ownerId, Money amount);
        Task<Notification> CreateWithdrawalNotificationAsync(Guid ownerId, Money amount);
    }
}