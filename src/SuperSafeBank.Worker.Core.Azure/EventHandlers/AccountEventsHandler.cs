using Azure.Data.Tables;
using MediatR;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Common;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.IntegrationEvents;
using SuperSafeBank.Service.Core.Azure.Common.Persistence;
using SuperSafeBank.Service.Core.Common.Queries;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSafeBank.Worker.Core.Azure.EventHandlers
{
    public class AccountEventsHandler :
        INotificationHandler<AccountCreated>,
        INotificationHandler<AccountUpdated>
    {
        private readonly IAggregateRepository<Customer, Guid> _customersRepo;
        private readonly IAggregateRepository<Account, Guid> _accountsRepo;
        private readonly IViewsContext _dbContext;
        private readonly ILogger<AccountEventsHandler> _logger;

        public AccountEventsHandler(
            IAggregateRepository<Customer, Guid> customersRepo,
            IAggregateRepository<Account, Guid> accountsRepo, 
            IViewsContext dbContext, 
            ILogger<AccountEventsHandler> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customersRepo = customersRepo ?? throw new ArgumentNullException(nameof(customersRepo));
            _accountsRepo = accountsRepo ?? throw new ArgumentNullException(nameof(accountsRepo));
        }

        public async Task Handle(AccountCreated @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("updating details for account {AccountId} ...", @event.AccountId);

            var accountView = await BuildAccountViewAsync(@event.AccountId, cancellationToken);
            await UpsertAccountViewAsync(accountView, cancellationToken);
        }

        public async Task Handle(AccountUpdated @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("processing transaction on account {AccountId} ...", @event.AccountId);

            var accountView = await BuildAccountViewAsync(@event.AccountId, cancellationToken);
            await UpsertAccountViewAsync(accountView, cancellationToken);
        }

        private async Task<AccountDetails> BuildAccountViewAsync(Guid accountId, CancellationToken cancellationToken)
        {
            var account = await _accountsRepo.RehydrateAsync(accountId, cancellationToken);
            var customer = await _customersRepo.RehydrateAsync(account.OwnerId, cancellationToken);

            var accountView = new AccountDetails(account.Id,
                account.OwnerId, customer.Firstname, customer.Lastname, customer.Email.Value,
                account.Balance);
            return accountView;
        }

        private async Task UpsertAccountViewAsync(AccountDetails accountView, CancellationToken cancellationToken)
        {
            var entity = ViewTableEntity.Map(accountView);
            var response = await _dbContext.Accounts.UpsertEntityAsync(entity, mode: TableUpdateMode.Replace, cancellationToken: cancellationToken);
            if (response?.Status != 204)
            {
                var msg = $"an error has occurred while processing an event: {response.ReasonPhrase}";
                throw new Exception(msg);
            }



            _logger.LogInformation("updated details for account {AccountId}", accountView.Id);
        }
    }
}