using SuperSafeBank.Common;
using SuperSafeBank.Persistence.Tests.Models;
using System.ComponentModel;

namespace SuperSafeBank.Persistence.SQLServer.Tests.Integration
{
    [Category("Integration")]
    [Trait("Category", "Integration")]
    public abstract class AggregateRepositoryBaseTests<TR>
        where TR : IAggregateRepository<DummyAggregate, Guid>
    {
        protected abstract ValueTask<TR> CreateSut();

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

            await Task.WhenAll(tasks);

            var rehydrated = await sut.RehydrateAsync(aggregateId);
            rehydrated.Should().NotBeNull();
            rehydrated.Version.Should().Be(3);
        }

        [Fact]
        public async Task PersistAsync_should_save_multiple_aggregates_concurrently()
        {
            var aggregates = Enumerable.Repeat(1, 10)
                                        .Select(i => new DummyAggregate(Guid.NewGuid()))
                                        .ToArray();

            var sut = await CreateSut();

            var tasks = aggregates
                .Select(a => sut.PersistAsync(a))
                .ToArray();

            await Task.WhenAll(tasks);

            foreach (var aggregate in aggregates)
            {
                var rehydrated = await sut.RehydrateAsync(aggregate.Id);
                rehydrated.Should().NotBeNull();
                rehydrated.Version.Should().Be(1);
            }
        }

        [Fact]
        public async Task RehydrateAsync_should_return_null_when_id_invalid()
        {
            var sut = await CreateSut();

            var result = await sut.RehydrateAsync(Guid.NewGuid()).ConfigureAwait(false);
            result.Should().BeNull();
        }
    }
}
