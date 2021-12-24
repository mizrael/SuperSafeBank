using FluentAssertions;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.IntegrationEvents;
using SuperSafeBank.Domain.Services;
using SuperSafeBank.Service.Core.Azure.Common.Persistence;
using SuperSafeBank.Service.Core.Azure.Tests;
using SuperSafeBank.Service.Core.Common.Queries;
using SuperSafeBank.Worker.Core.Azure.EventHandlers;
using System;
using System.ComponentModel;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SuperSafeBank.Worker.Core.Azure.Tests.Integration.EventHandlers
{
    [Trait("Category", "Integration")]
    [Category("Integration")]
    public class CustomerDetailsHandlerTests : IClassFixture<StorageTableFixutre>
    {
        private readonly StorageTableFixutre _fixture;

        public CustomerDetailsHandlerTests(StorageTableFixutre fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Handle_CustomerCreated_should_create_view()
        {
            var dbContext = _fixture.CreateTableClient();
            var customersRepo = _fixture.CreateRepository<Customer, Guid>();
            var accountsRepo = _fixture.CreateRepository<Account, Guid>();

            var customer = Customer.Create(Guid.NewGuid(), "lorem", "ipsum", "test@email.com");
            await customersRepo.PersistAsync(customer);

            var currencyConverter = new FakeCurrencyConverter();

            var @event = new CustomerCreated(Guid.NewGuid(), customer.Id);

            var logger = NSubstitute.Substitute.For<ILogger<CustomerDetailsHandler>>();
            var sut = new CustomerDetailsHandler(customersRepo, accountsRepo, dbContext, currencyConverter, logger);
            await sut.Handle(@event, CancellationToken.None);

            var key = customer.Id.ToString();
            var response = await dbContext.CustomersDetails.GetEntityAsync<ViewTableEntity>(key, key);
            response.Should().NotBeNull();
            response.Value.Should().NotBeNull();
            var customerView = JsonSerializer.Deserialize<CustomerDetails>(response.Value.Data);
            customerView.Should().NotBeNull();
            customerView.Id.Should().Be(customer.Id);
            customerView.Firstname.Should().Be(customer.Firstname);
            customerView.Lastname.Should().Be(customer.Lastname);
            customerView.Email.Should().Be(customer.Email.Value);
            customerView.Accounts.Should().NotBeNull().And.HaveCount(0);
            customerView.TotalBalance.Should().Be(Money.Zero(Currency.CanadianDollar));
        }
    }
}
