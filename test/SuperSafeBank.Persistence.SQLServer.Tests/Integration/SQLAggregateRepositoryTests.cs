using Microsoft.Data.SqlClient;
using SuperSafeBank.Common;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.DomainEvents;
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

            var sut = await CreateSut();

            await sut.PersistAsync(aggregate);

            var rehydrated = await sut.RehydrateAsync(aggregate.Id);
            rehydrated.Should().NotBeNull();
            rehydrated.Version.Should().Be(3);
        }

        [Fact]
        public async Task PersistAsync_should_not_save_aggregate_when_version_mismatch()
        {
            var aggregateId = Guid.NewGuid();
            var aggregate = new DummyAggregate(aggregateId);
            aggregate.DoSomething("foo");
            aggregate.DoSomething("bar");

            var invalidAggregate = new DummyAggregate(aggregateId);
            invalidAggregate.DoSomething("nope");

            var sut = await CreateSut();

            await sut.PersistAsync(aggregate);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await sut.PersistAsync(invalidAggregate));
        }

        [Fact]
        public async Task PersistAsync_should_save_aggregate_only_once_if_multiple_concurrent_calls()
        {
            var aggregateId = Guid.NewGuid();
            var aggregate = new DummyAggregate(aggregateId);
            aggregate.DoSomething("foo");
            aggregate.DoSomething("bar");

            var sut = await CreateSut();

            var tasks = Enumerable.Repeat(1, 5)
                .Select(_ => sut.PersistAsync(aggregate))
                .ToArray();

            await Assert.ThrowsAsync<SqlException>(async () => await Task.WhenAll(tasks));

            var rehydrated = await sut.RehydrateAsync(aggregateId);
            rehydrated.Should().NotBeNull();
            rehydrated.Version.Should().Be(3);
        }

        [Fact]
        public async Task RehydrateAsync_should_return_null_when_id_invalid()
        {
            var sut = await CreateSut();

            var result = await sut.RehydrateAsync(Guid.NewGuid()).ConfigureAwait(false);
            result.Should().BeNull();
        }

        private async Task<SQLAggregateRepository<DummyAggregate, Guid>> CreateSut()
        {
            var conn = await _fixture.CreateDbConnectionStringProviderAsync();
            var tableCreator = new AggregateTableCreator(conn);
            var serializer = new JsonEventSerializer(new[]
            {
                typeof(DummyAggregate).Assembly
            });
            var sut = new SQLAggregateRepository<DummyAggregate, Guid>(conn, tableCreator, serializer);
            return sut;
        }
    }
}
