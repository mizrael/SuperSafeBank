using SuperSafeBank.Persistence.Tests.Models;
using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Common;
using System.ComponentModel;

namespace SuperSafeBank.Persistence.EventStore.Tests.Integration;

[Trait("Category", "Integration")]
[Category("Integration")]
public class EventsRepositoryTests : IClassFixture<EventStoreFixture>
{
    private readonly EventStoreFixture _fixture;

    public EventsRepositoryTests(EventStoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task PersistAsync_should_store_events()
    {
        var connStr = new Uri(_fixture.ConnectionString);
        var logger = NSubstitute.Substitute.For<ILogger<EventStoreConnectionWrapper>>();
        using var conn = new EventStoreConnectionWrapper(connStr, logger);

        var serializer = new JsonEventSerializer(new[]
        {
            typeof(DummyAggregate).Assembly
        });

        var sut = new EventStoreAggregateRepository<DummyAggregate, Guid>(conn, serializer);

        var aggregate = new DummyAggregate(Guid.NewGuid());
        aggregate.DoSomething("foo");
        aggregate.DoSomething("bar");

        await sut.PersistAsync(aggregate);

        var rehydrated = await sut.RehydrateAsync(aggregate.Id);        
        rehydrated.Should().NotBeNull();
        rehydrated.Id.Should().Be(aggregate.Id);
        rehydrated.Version.Should().Be(3);
    }

    [Fact]
    public async Task PersistAsync_should_clear_Aggregate_events()
    {
        var connStr = new Uri(_fixture.ConnectionString);
        var logger = NSubstitute.Substitute.For<ILogger<EventStoreConnectionWrapper>>();
        using var conn = new EventStoreConnectionWrapper(connStr, logger);

        var serializer = new JsonEventSerializer(new[]
        {
            typeof(DummyAggregate).Assembly
        });

        var sut = new EventStoreAggregateRepository<DummyAggregate, Guid>(conn, serializer);

        var aggregate = new DummyAggregate(Guid.NewGuid());
        aggregate.DoSomething("foo");
        aggregate.DoSomething("bar");

        aggregate.Events.Should().NotBeEmpty();

        await sut.PersistAsync(aggregate);

        aggregate.Events.Should().BeEmpty();
    }

    [Fact]
    public async Task RehydrateAsync_should_return_null_when_id_invalid()
    {
        var connStr = new Uri(_fixture.ConnectionString);
        var logger = NSubstitute.Substitute.For<ILogger<EventStoreConnectionWrapper>>();
        using var conn = new EventStoreConnectionWrapper(connStr, logger);

        var serializer = new JsonEventSerializer(new[]
        {
            typeof(DummyAggregate).Assembly
        });

        var sut = new EventStoreAggregateRepository<DummyAggregate, Guid>(conn, serializer);

        var rehydrated = await sut.RehydrateAsync(Guid.NewGuid());
        rehydrated.Should().BeNull();
    }
}
