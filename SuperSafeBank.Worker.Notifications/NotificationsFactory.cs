using System;
using System.Threading.Tasks;
using SuperSafeBank.Domain;
using SuperSafeBank.Worker.Notifications.ApiClients;

namespace SuperSafeBank.Worker.Notifications
{
    public class NotificationsFactory : INotificationsFactory
    {
        private readonly ICustomersApiClient _customersApiClient;

        public NotificationsFactory(ICustomersApiClient customersApiClient)
        {
            _customersApiClient = customersApiClient ?? throw new ArgumentNullException(nameof(customersApiClient));
        }

        public async Task<Notification> CreateNewAccountNotificationAsync(Guid customerId, Guid accountId)
        {
            var customer = await _customersApiClient.GetCustomerAsync(customerId);
            var message = $"dear {customer.Firstname}, a new account was created for you: {accountId}";
            return new Notification(customer.Email, message);
        }

        public async Task<Notification> CreateDepositNotificationAsync(Guid ownerId, Money amount)
        {
            var customer = await _customersApiClient.GetCustomerAsync(ownerId);
            var message = $"dear {customer.Firstname}, a deposit of {amount} was done on your account";
            return new Notification(customer.Email, message);
        }

        public async Task<Notification> CreateWithdrawalNotificationAsync(Guid ownerId, Money amount)
        {
            var customer = await _customersApiClient.GetCustomerAsync(ownerId);
            var message = $"dear {customer.Firstname}, a withdrawal of {amount} was done from your account";
            return new Notification(customer.Email, message);
        }
    }
}