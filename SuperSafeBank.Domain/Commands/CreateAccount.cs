using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SuperSafeBank.Core;

namespace SuperSafeBank.Domain.Commands
{
    public class CreateAccount : INotification
    {
        public CreateAccount(Guid customerId, Guid accountId, Currency currency)
        {
            CustomerId = customerId;
            AccountId = accountId;
            Currency = currency;
        }
        public Guid CustomerId { get; }
        public Guid AccountId { get; }
        public Currency Currency { get; }
    }

    public class CreateAccountHandler : INotificationHandler<CreateAccount>
    {
        private readonly IEventsService<Customer, Guid> _customerEventsService;
        private readonly IEventsService<Account, Guid> _accountEventsService;

        public CreateAccountHandler(IEventsService<Customer, Guid> customerEventsService, IEventsService<Account, Guid> accountEventsService)
        {
            _customerEventsService = customerEventsService;
            _accountEventsService = accountEventsService;
        }

        public async Task Handle(CreateAccount command, CancellationToken cancellationToken)
        {
            var customer = await _customerEventsService.RehydrateAsync(command.CustomerId);
            if(null == customer)
                throw new ArgumentOutOfRangeException(nameof(CreateAccount.CustomerId), "invalid customer id");
          
            var account = new Account(command.AccountId, customer, command.Currency);
            await _accountEventsService.PersistAsync(account);
        }
    }
}