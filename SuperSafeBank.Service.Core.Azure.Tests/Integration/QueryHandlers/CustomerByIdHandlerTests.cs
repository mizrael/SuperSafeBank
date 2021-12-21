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
    public class CustomerByIdHandlerTests : IClassFixture<StorageTableFixutre>
    {
        private readonly StorageTableFixutre _fixture;

        public CustomerByIdHandlerTests(StorageTableFixutre fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Handle_should_return_null_when_input_invalid()
        {
            var query = new CustomerById(Guid.Empty); 
            
            var dbContext = _fixture.CreateTableClient();
            var sut = new CustomerByIdHandler(dbContext);
            var result = await sut.Handle(query, CancellationToken.None);
            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_should_return_model_when_input_valid()
        {
            var customer = new CustomerDetails(Guid.NewGuid(), "test", "customer", "test@email.com", null, Money.Zero(Currency.CanadianDollar));
            var entity = ViewTableEntity.Map(customer);

            var dbContext = _fixture.CreateTableClient();
            await dbContext.CustomersDetails.UpsertEntityAsync(entity);

            var query = new CustomerById(customer.Id);
            
            var sut = new CustomerByIdHandler(dbContext);
            var result = await sut.Handle(query, CancellationToken.None);
            result.Should().NotBeNull();
            result.Firstname.Should().Be(customer.Firstname);
            result.Lastname.Should().Be(customer.Lastname);
            result.Email.Should().Be(customer.Email);
            result.TotalBalance.Should().Be(customer.TotalBalance);
            result.Accounts.Should().BeEmpty();
        }
    }
}
