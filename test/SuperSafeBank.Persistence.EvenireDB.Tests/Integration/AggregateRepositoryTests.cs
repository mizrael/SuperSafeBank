using EvenireDB.Client;
using Microsoft.Extensions.DependencyInjection;
using SuperSafeBank.Common;
using SuperSafeBank.Common.Models;
using SuperSafeBank.Persistence.Tests.Models;
using FluentAssertions;
using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace SuperSafeBank.Persistence.EvenireDB.Tests.Integration;

[Trait("Category", "Integration")]
[Category("Integration")]
public class AggregateRepositoryTests
{
    [Fact]
    public async Task PersistAsync_should_store_events()
    {
        var sut = CreateSUT<DummyAggregate, Guid>();

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
        var sut = CreateSUT<DummyAggregate, Guid>();

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
        var sut = CreateSUT<DummyAggregate, Guid>();

        var rehydrated = await sut.RehydrateAsync(Guid.NewGuid());
        rehydrated.Should().BeNull();
    }

    private static AggregateRepository<TA, TKey> CreateSUT<TA, TKey>()
        where TA: class, IAggregateRoot<TKey>
    {
        var config = new EvenireClientConfig()
        {
            ServerUri = new Uri("http://localhost"),
            UseGrpc = false,
            HttpSettings = new EvenireClientConfig.HttpTransportSettings()
            {
                Port = 5001
            }            
        };
          
        var sp = new ServiceCollection()
            .AddEvenireDB(config)     
            .AddSingleton<IEventSerializer>(_ => new JsonEventSerializer(new[]
            {
                typeof(DummyAggregate).Assembly
            }))
            .BuildServiceProvider();
        var client = sp.GetRequiredService<IEventsClient>();
        var serializer = sp.GetRequiredService<IEventSerializer>();
        return new AggregateRepository<TA, TKey>(client, serializer);
    }
}