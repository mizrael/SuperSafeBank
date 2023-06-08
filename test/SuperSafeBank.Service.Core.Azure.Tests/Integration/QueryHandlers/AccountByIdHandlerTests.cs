using FluentAssertions;
using SuperSafeBank.Domain;
using SuperSafeBank.Service.Core.Azure.Common.Persistence;
using SuperSafeBank.Service.Core.Azure.QueryHandlers;
using SuperSafeBank.Service.Core.Common.Queries;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SuperSafeBank.Service.Core.Azure.Tests.Integration.QueryHandlers
{
    [Trait("Category", "Integration")]
    [Category("Integration")]
    public class AccountByIdHandlerTests : IClassFixture<StorageTableFixutre>
    {
        private readonly StorageTableFixutre _fixture;

        public AccountByIdHandlerTests(StorageTableFixutre fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Handle_should_return_null_when_input_invalid()
        {
            var query = new AccountById(Guid.Empty); 
            
            var dbContext = _fixture.CreateTableClient();
            var sut = new AccountByIdHandler(dbContext);
            var result = await sut.Handle(query, CancellationToken.None);
            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_should_return_model_when_input_valid()
        {
            var account = new AccountDetails(Guid.NewGuid(), Guid.NewGuid(), "test", "customer", "test@email.com", Money.Zero(Currency.CanadianDollar));
            var entity = ViewTableEntity.Map(account);

            var dbContext = _fixture.CreateTableClient();
            await dbContext.Accounts.UpsertEntityAsync(entity);

            var query = new AccountById(account.Id);
            
            var sut = new AccountByIdHandler(dbContext);
            var result = await sut.Handle(query, CancellationToken.None);
            result.Should().NotBeNull();
            result.OwnerEmail.Should().Be(account.OwnerEmail);
            result.OwnerLastName.Should().Be(account.OwnerLastName);
            result.OwnerFirstName.Should().Be(account.OwnerFirstName);
            result.OwnerId.Should().Be(account.OwnerId);
            result.Id.Should().Be(account.Id);
            result.Balance.Should().Be(account.Balance);
        }
    }
}
