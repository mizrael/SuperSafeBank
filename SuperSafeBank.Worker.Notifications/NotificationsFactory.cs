using System;
using System.Threading.Tasks;
using SuperSafeBank.Domain;
using SuperSafeBank.Worker.Notifications.ApiClients;

namespace SuperSafeBank.Worker.Notifications
{
    public class NotificationsFactory : INotificationsFactory
    {
        private readonly ICustomersApiClient _customersApiClient;
        private readonly IAccountsApiClient _accountsApiClient;

        public NotificationsFactory(ICustomersApiClient customersApiClient, IAccountsApiClient accountsApiClient)
        {
            _customersApiClient = customersApiClient ?? throw new ArgumentNullException(nameof(customersApiClient));
            _accountsApiClient = accountsApiClient ?? throw new ArgumentNullException(nameof(accountsApiClient));
        }

        public async Task<Notification> CreateNewAccountNotificationAsync(Guid customerId, Guid accountId)
        {
            var customer = await _customersApiClient.GetCustomerAsync(customerId);
            var message = $"dear {customer.Firstname}, a new account was created for you: {accountId}";
            return new Notification(customer.Email, message);
        }

        public async Task<Notification> CreateDepositNotificationAsync(Guid accountId, Money amount)
        {
            var account = await _accountsApiClient.GetAccountAsync(accountId);
            var message = $"dear {account.OwnerFirstName}, a deposit of {amount} was done on your account";
            return new Notification(account.OwnerEmail, message);
        }

        public async Task<Notification> CreateWithdrawalNotificationAsync(Guid accountId, Money amount)
        {
            var account = await _accountsApiClient.GetAccountAsync(accountId);
            var message = $"dear {account.OwnerFirstName}, a withdrawal of {amount} was done from your account";
            return new Notification(account.OwnerEmail, message);
        }
    }
}