using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Core.EventBus;
using SuperSafeBank.Core.Models;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.Events;
using SuperSafeBank.Persistence.Kafka;

namespace SuperSafeBank.Worker.Notifications
{
    public class AccountEventsWorker : BackgroundService
    {
        private readonly EventConsumer<Account, Guid> _consumer;
        private readonly ILogger<IEventConsumer> _logger;
        private readonly INotificationsService _notificationsService;
        private readonly INotificationsFactory _notificationsFactory;

        public AccountEventsWorker(INotificationsFactory notificationsFactory,
            INotificationsService notificationsService, EventConsumer<Account, Guid> consumer,
            ILogger<IEventConsumer> logger)
        {
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _logger = logger;
            _notificationsService = notificationsService;
            _notificationsFactory = notificationsFactory;

            consumer.EventReceived += OnEventReceived;

            consumer.ExceptionThrown += (sender, exception) =>
            {
                logger.LogError(exception, $"an exception has occurred while consuming a message: {exception.Message}");
            };
        }

        private async Task OnEventReceived(object s, IDomainEvent<Guid> @event)
        {
            var notification = @event switch
            {
                AccountCreated newAccount => await _notificationsFactory.CreateNewAccountNotificationAsync(newAccount.OwnerId, newAccount.AggregateId),
                Deposit deposit => await _notificationsFactory.CreateDepositNotificationAsync(deposit.OwnerId, deposit.Amount),
                Withdrawal withdrawal => await _notificationsFactory.CreateWithdrawalNotificationAsync(withdrawal.OwnerId, withdrawal.Amount),
                _ => null
            };

            if (null != notification)
                await _notificationsService.DispatchAsync(notification);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _consumer.ConsumeAsync(stoppingToken);
        }
    }
}