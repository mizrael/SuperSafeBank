using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Common.EventBus;
using SuperSafeBank.Domain.IntegrationEvents;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSafeBank.Worker.Notifications
{
    public class AccountEventsWorker : BackgroundService
    {
        private readonly IEventConsumer _consumer;
        private readonly ILogger<AccountEventsWorker> _logger;
        private readonly INotificationsService _notificationsService;
        private readonly INotificationsFactory _notificationsFactory;

        public AccountEventsWorker(INotificationsFactory notificationsFactory,
            INotificationsService notificationsService, 
            IEventConsumer consumer,
            ILogger<AccountEventsWorker> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notificationsService = notificationsService ?? throw new ArgumentNullException(nameof(notificationsService));
            _notificationsFactory = notificationsFactory ?? throw new ArgumentNullException(nameof(notificationsFactory));

            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _consumer.EventReceived += OnEventReceived;
            _consumer.ExceptionThrown += OnExceptionThrown;
        }

        private async Task OnEventReceived(object s, IIntegrationEvent @event)
        {
            var notification = @event switch
            {
                AccountCreated newAccount => await _notificationsFactory.CreateNewAccountNotificationAsync(newAccount.AccountId),
                TransactionHappened transaction => await _notificationsFactory.CreateTransactionNotificationAsync(transaction.AccountId),
                _ => (Notification)null
            };

            if (null != notification)
                await _notificationsService.DispatchAsync(notification);
        }

        private void OnExceptionThrown(object s, Exception ex)
            => _logger.LogError(ex, $"an exception has occurred while consuming a message: {ex.Message}");

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) 
            => await _consumer.StartConsumeAsync(stoppingToken);

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _consumer.EventReceived -= OnEventReceived;
            _consumer.ExceptionThrown -= OnExceptionThrown;

            return base.StopAsync(cancellationToken);
        }
    }
}