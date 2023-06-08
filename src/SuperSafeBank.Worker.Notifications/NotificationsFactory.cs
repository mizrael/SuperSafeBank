using SuperSafeBank.Worker.Notifications.ApiClients;
using System;
using System.Threading.Tasks;

namespace SuperSafeBank.Worker.Notifications
{
    public class NotificationsFactory : INotificationsFactory
    {
        private readonly IAccountsApiClient _accountsApiClient;

        public NotificationsFactory(IAccountsApiClient accountsApiClient)
        {     
            _accountsApiClient = accountsApiClient ?? throw new ArgumentNullException(nameof(accountsApiClient));
        }

        public async Task<Notification> CreateNewAccountNotificationAsync(Guid accountId)
        {
            var account = await _accountsApiClient.GetAccountAsync(accountId);            
            var message = $"dear {account.OwnerFirstName}, a new account was created for you: {accountId}";
            return new Notification(account.OwnerEmail, message);
        }

        public async Task<Notification> CreateTransactionNotificationAsync(Guid accountId)
        {
            var account = await _accountsApiClient.GetAccountAsync(accountId);
            var message = $"dear {account.OwnerFirstName}, a transaction occurred on your account {account.Id}";
            return new Notification(account.OwnerEmail, message);
        }
    }
}