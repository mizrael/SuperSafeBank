using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SuperSafeBank.Common;
using SuperSafeBank.Domain.Services;

namespace SuperSafeBank.Domain.Commands
{
    public record Withdraw : INotification
    {
        public Withdraw(Guid accountId, Money amount)
        {
            AccountId = accountId;
            Amount = amount ?? throw new ArgumentNullException(nameof(amount));
        }

        public Guid AccountId { get; }
        public Money Amount { get; }
    }


    public class WithdrawHandler : INotificationHandler<Withdraw>
    {
        private readonly IEventsService<Account, Guid> _accountEventsService;
        private readonly ICurrencyConverter _currencyConverter;

        public WithdrawHandler(IEventsService<Account, Guid> accountEventsService, ICurrencyConverter currencyConverter)
        {
            _accountEventsService = accountEventsService;
            _currencyConverter = currencyConverter;
        }

        public async Task Handle(Withdraw command, CancellationToken cancellationToken)
        {
            var account = await _accountEventsService.RehydrateAsync(command.AccountId);
            if (null == account)
                throw new ArgumentOutOfRangeException(nameof(Withdraw.AccountId), "invalid account id");

            account.Withdraw(command.Amount, _currencyConverter);

            await _accountEventsService.PersistAsync(account);
        }
    }
}