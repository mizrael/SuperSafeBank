using SuperSafeBank.Persistence.Tests.Models;
using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Core;

namespace SuperSafeBank.Persistence.EventStore.Tests
{
    public class EventsRepositoryTests
    {
        [Fact]
        public async Task AppendAsync_should_store_events()
        {
            var connStr = new Uri(Settings.EventStoreConnectionString);
            var logger = NSubstitute.Substitute.For<ILogger<EventStoreConnectionWrapper>>();
            using var conn = new EventStoreConnectionWrapper(connStr, logger);

            var serializer = NSubstitute.Substitute.For<IEventSerializer>();

            var sut = new EventsRepository<DummyAggregate, Guid>(conn, serializer);

            var aggregate = new DummyAggregate(Guid.NewGuid());
            aggregate.DoSomething("foo");
            aggregate.DoSomething("bar");

            await sut.AppendAsync(aggregate);

            var rehydrated = await sut.RehydrateAsync(aggregate.Id);
            rehydrated.Should().NotBeNull();
            rehydrated.Version.Should().Be(3);
        }

        [Fact]
        public async Task RehydrateAsync_should_return_null_when_id_invalid()
        {
            var connStr = new Uri(Settings.EventStoreConnectionString);
            var logger = NSubstitute.Substitute.For<ILogger<EventStoreConnectionWrapper>>();
            using var conn = new EventStoreConnectionWrapper(connStr, logger);

            var serializer = NSubstitute.Substitute.For<IEventSerializer>();

            var sut = new EventsRepository<DummyAggregate, Guid>(conn, serializer);

            var rehydrated = await sut.RehydrateAsync(Guid.NewGuid());
            rehydrated.Should().BeNull();
        }
    }
}
