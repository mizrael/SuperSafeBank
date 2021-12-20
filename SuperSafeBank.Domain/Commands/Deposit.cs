using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SuperSafeBank.Common;
using SuperSafeBank.Domain.Services;

namespace SuperSafeBank.Domain.Commands
{
    public class Deposit : INotification
    {
        public Deposit(Guid accountId, Money amount)
        {
            AccountId = accountId;
            Amount = amount;
        }

        public Guid AccountId { get; }
        public Money Amount { get; }
    }

    public class DepositHandler : INotificationHandler<Deposit>
    {
        private readonly IEventsService<Account, Guid> _accountEventsService;
        private readonly ICurrencyConverter _currencyConverter;

        public DepositHandler(IEventsService<Account, Guid> accountEventsService, ICurrencyConverter currencyConverter)
        {
            _accountEventsService = accountEventsService;
            _currencyConverter = currencyConverter;
        }

        public async Task Handle(Deposit command, CancellationToken cancellationToken)
        {
            var account = await _accountEventsService.RehydrateAsync(command.AccountId);
            if(null == account)
                throw new ArgumentOutOfRangeException(nameof(Deposit.AccountId), "invalid account id");

            account.Deposit(command.Amount, _currencyConverter);

            await _accountEventsService.PersistAsync(account);
        }
    }

}