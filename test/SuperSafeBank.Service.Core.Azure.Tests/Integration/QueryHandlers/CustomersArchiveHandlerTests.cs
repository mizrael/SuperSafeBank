using FluentAssertions;
using SuperSafeBank.Service.Core.Azure.Common.Persistence;
using SuperSafeBank.Service.Core.Azure.QueryHandlers;
using SuperSafeBank.Service.Core.Common.Queries;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SuperSafeBank.Service.Core.Azure.Tests.Integration.QueryHandlers
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
        public async Task Handle_should_return_empty_collection_when_items_not_available()
        {
            var query = new CustomersArchive();

            var dbContext = _fixture.CreateTableClient();
            var sut = new CustomersArchiveHandler(dbContext);
            var result = await sut.Handle(query, CancellationToken.None);
            result.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public async Task Handle_should_return_results_when_available()
        {
            var entities = Enumerable.Range(0, 10)
                .Select(_ =>
                {
                    var customer = new CustomerArchiveItem(Guid.NewGuid(), "test", "customer");
                    return ViewTableEntity.Map(customer);
                });
            var dbContext = _fixture.CreateTableClient();
            foreach(var entity in entities)
                await dbContext.CustomersArchive.AddEntityAsync(entity);

            var query = new CustomersArchive();

            var sut = new CustomersArchiveHandler(dbContext);
            var result = await sut.Handle(query, CancellationToken.None);
            result.Should().NotBeNullOrEmpty()
                .And.HaveCount(entities.Count());
        }
    }
}
