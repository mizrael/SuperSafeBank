using FluentAssertions;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Common.EventBus;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.Events;
using SuperSafeBank.Service.Core.Azure.Common.Persistence;
using SuperSafeBank.Service.Core.Azure.Tests;
using SuperSafeBank.Service.Core.Common.Queries;
using SuperSafeBank.Worker.Core.Azure.EventHandlers;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SuperSafeBank.Worker.Core.Azure.Tests.Integration.EventHandlers
{
    [Trait("Category", "Integration")]
    [Category("Integration")]
    public class CustomersArchiveHandlerTests : IClassFixture<StorageTableFixutre>
    {
        private readonly StorageTableFixutre _fixture;

        public CustomersArchiveHandlerTests(StorageTableFixutre fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Handle_CustomerCreated_should_create_view()
        {
            var customer = Customer.Create(Guid.NewGuid(), "lorem", "ipsum", "test@email.com");
            var customerCreated = customer.Events.First(evt => evt is CustomerCreated) as CustomerCreated;
            var evt = new EventReceived<CustomerCreated>(customerCreated); 
            
            var dbContext = _fixture.CreateTableClient();
            var logger = NSubstitute.Substitute.For<ILogger<CustomersArchiveHandler>>();
            var sut = new CustomersArchiveHandler(dbContext, logger);
            await sut.Handle(evt, CancellationToken.None);

            var key = customer.Id.ToString();
            var response = await dbContext.CustomersArchive.GetEntityAsync<ViewTableEntity>(key, key);
            response.Should().NotBeNull();
            response.Value.Should().NotBeNull();
            var customerView = JsonSerializer.Deserialize<CustomerArchiveItem>(response.Value.Data);
            customerView.Should().NotBeNull();
            customerView.Id.Should().Be(customer.Id);
            customerView.Firstname.Should().Be(customer.Firstname);
            customerView.Lastname.Should().Be(customer.Lastname);
        }
    }
}
