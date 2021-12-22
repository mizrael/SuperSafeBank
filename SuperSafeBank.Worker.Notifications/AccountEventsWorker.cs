using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Common.EventBus;
using SuperSafeBank.Common.Models;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSafeBank.Worker.Notifications
{
    public class AccountEventsWorker : BackgroundService
    {
        private readonly IEventConsumer<Account, Guid> _consumer;
        private readonly ILogger<AccountEventsWorker> _logger;
        private readonly INotificationsService _notificationsService;
        private readonly INotificationsFactory _notificationsFactory;

        public AccountEventsWorker(INotificationsFactory notificationsFactory,
            INotificationsService notificationsService, 
            IEventConsumer<Account, Guid> consumer,
            ILogger<AccountEventsWorker> logger)
        {
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            this._logger = logger;
            _notificationsService = notificationsService;
            _notificationsFactory = notificationsFactory;

            consumer.EventReceived += OnEventReceived;
            consumer.ExceptionThrown += OnExceptionThrown;
        }

        private async Task OnEventReceived(object s, IDomainEvent<Guid> @event)
        {
            var notification = @event switch
            {
                AccountCreated newAccount => await _notificationsFactory.CreateNewAccountNotificationAsync(newAccount.OwnerId, newAccount.AggregateId),
                Deposit deposit => await _notificationsFactory.CreateDepositNotificationAsync(deposit.AggregateId, deposit.Amount),
                Withdrawal withdrawal => await _notificationsFactory.CreateWithdrawalNotificationAsync(withdrawal.AggregateId, withdrawal.Amount),
                _ => null
            };

            if (null != notification)
                await _notificationsService.DispatchAsync(notification);
        }

        private void OnExceptionThrown(object s, Exception ex)
        {
            _logger.LogError(ex, $"an exception has occurred while consuming a message: {ex.Message}");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _consumer.ConsumeAsync(stoppingToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _consumer.EventReceived -= OnEventReceived;
            _consumer.ExceptionThrown -= OnExceptionThrown;

            return base.StopAsync(cancellationToken);
        }
    }
}