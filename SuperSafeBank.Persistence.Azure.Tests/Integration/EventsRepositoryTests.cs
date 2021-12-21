using FluentAssertions;
using SuperSafeBank.Common;
using SuperSafeBank.Persistence.Azure.Tests.Integration.Fixtures;
using SuperSafeBank.Persistence.Tests.Models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SuperSafeBank.Persistence.Azure.Tests.Integration
{
    [Trait("Category", "Integration")]
    [Category("Integration")]
    public class EventsRepositoryTests : IClassFixture<StorageTableFixutre>
    {
        private readonly StorageTableFixutre _fixture;
        private static readonly JsonEventSerializer _eventSerializer;
        
        static EventsRepositoryTests()
        {
            _eventSerializer = new JsonEventSerializer(new[] {typeof(DummyAggregate).Assembly});
        }

        public EventsRepositoryTests(StorageTableFixutre fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task AppendAsync_should_store_events()
        {
            var client = await _fixture.CreateTableClientAsync();

            var sut = new EventsRepository<DummyAggregate, Guid>(client, _eventSerializer);

            var aggregate = new DummyAggregate(Guid.NewGuid());
            aggregate.DoSomething("foo");
            aggregate.DoSomething("bar");

            await sut.AppendAsync(aggregate);

            var events = client.QueryAsync<EventData<Guid>>(ed => ed.PartitionKey == aggregate.Id.ToString())
                                .ConfigureAwait(false);
            int count = 0;
            await foreach(var evt in events)
            {
                count++;
            }
            count.Should().Be(3);
        }

        [Fact]
        public async Task AppendAsync_should_throw_AggregateException_when_version_mismatch()
        {
            var db = await _fixture.CreateTableClientAsync();

            var sut = new EventsRepository<DummyAggregate, Guid>(db, _eventSerializer);

            var aggregateId = Guid.NewGuid();

            var tasks = Enumerable.Range(1, 3)
                .Select(i =>
                {
                    var aggregate = new DummyAggregate(aggregateId);
                    aggregate.DoSomething($"foo|{i}");
                    return sut.AppendAsync(aggregate);
                }).ToArray();

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await Task.WhenAll(tasks));
        }

        [Fact]
        public async Task RehydrateAsync_should_store_events()
        {
            var db = await _fixture.CreateTableClientAsync();

            var sut = new EventsRepository<DummyAggregate, Guid>(db, _eventSerializer);

            var aggregate = new DummyAggregate(Guid.NewGuid());
            aggregate.DoSomething("lorem");
            aggregate.DoSomething("ipsum");

            await sut.AppendAsync(aggregate);

            var rehydrated = await sut.RehydrateAsync(aggregate.Id);
            rehydrated.Should().NotBeNull();
            rehydrated.Id.Should().Be(aggregate.Id);
            rehydrated.Events.Should().BeEmpty();
            rehydrated.WhatHappened.Should().HaveCount(3)
                .And.Contain("created")
                .And.Contain("lorem")
                .And.Contain("ipsum");
        }
    }
}