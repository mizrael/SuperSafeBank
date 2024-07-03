using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SuperSafeBank.Common;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.IntegrationEvents;
using SuperSafeBank.Service.Core.Common.Queries;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSafeBank.Service.Core.Persistence.Mongo.EventHandlers;

public class AccountEventsHandler : 
    INotificationHandler<AccountCreated>,
    INotificationHandler<AccountUpdated>
{
    private readonly IQueryDbContext _db;
    private readonly IAggregateRepository<Customer, Guid> _customersRepo;
    private readonly IAggregateRepository<Account, Guid> _accountsRepo;
    private readonly ILogger<AccountEventsHandler> _logger;

    public AccountEventsHandler(
        IQueryDbContext db,
        IAggregateRepository<Customer, Guid> customersRepo,
        IAggregateRepository<Account, Guid> accountsRepo,
        ILogger<AccountEventsHandler> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _customersRepo = customersRepo ?? throw new ArgumentNullException(nameof(customersRepo));
        _accountsRepo = accountsRepo ?? throw new ArgumentNullException(nameof(accountsRepo));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        var filter = Builders<AccountDetails>.Filter
            .Eq(a => a.Id, accountView.Id);

        var update = Builders<AccountDetails>.Update
            .Set(a => a.Id, accountView.Id)
            .Set(a => a.OwnerFirstName, accountView.OwnerFirstName)
            .Set(a => a.OwnerLastName, accountView.OwnerLastName)
            .Set(a => a.OwnerEmail, accountView.OwnerEmail)
            .Set(a => a.OwnerId, accountView.OwnerId)
            .Set(a => a.Balance, accountView.Balance);

        await _db.AccountsDetails.UpdateOneAsync(filter,
            cancellationToken: cancellationToken,
            update: update,
            options: new UpdateOptions() { IsUpsert = true });

        _logger.LogInformation("updated details for account {AccountId}", accountView.Id);
    }
}
