using SuperSafeBank.Common;
using SuperSafeBank.Domain;
using SuperSafeBank.Persistence.Tests.Models;
using System.ComponentModel;

namespace SuperSafeBank.Persistence.SQLServer.Tests.Integration
{
    [Category("Integration")]
    [Trait("Category", "Integration")]
    public class SQLAggregateRepositoryTests : IClassFixture<DbFixture>
    {
        private readonly DbFixture _fixture;

        public SQLAggregateRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task PersistAsync_should_save_aggregate()
        {
            var aggregate = new DummyAggregate(Guid.NewGuid());
            aggregate.DoSomething("foo");
            aggregate.DoSomething("bar");

            using var conn = await _fixture.CreateDbConnectionAsync();
            var tableCreator = new AggregateTableCreator(conn);
            var serializer = NSubstitute.Substitute.For<IEventSerializer>();
            var sut = new SQLAggregateRepository<DummyAggregate, Guid>(conn, tableCreator, serializer);

            await sut.PersistAsync(aggregate);

            var rehydrated = await sut.RehydrateAsync(aggregate.Id);
            rehydrated.Should().NotBeNull();
            rehydrated.Version.Should().Be(3);
        }

        [Fact]
        public async Task RehydrateAsync_should_return_null_when_id_invalid()
        {
            using var conn = await _fixture.CreateDbConnectionAsync();
            var tableCreator = new AggregateTableCreator(conn);
            var serializer = NSubstitute.Substitute.For<IEventSerializer>();
            var sut = new SQLAggregateRepository<Customer, Guid>(conn, tableCreator, serializer);

            var result = await sut.RehydrateAsync(Guid.NewGuid()).ConfigureAwait(false);
            result.Should().BeNull();
        }
    }
}
