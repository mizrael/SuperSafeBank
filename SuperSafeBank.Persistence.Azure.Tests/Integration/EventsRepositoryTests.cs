using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Cosmos.Linq;
using SuperSafeBank.Core;
using SuperSafeBank.Persistence.Azure.Tests.Integration.Fixtures;
using SuperSafeBank.Persistence.Azure.Tests.Models;
using Xunit;

namespace SuperSafeBank.Persistence.Azure.Tests.Integration
{
    public class EventsRepositoryTests : IClassFixture<CosmosFixture>
    {
        private readonly CosmosFixture _fixture;
        private static readonly JsonEventSerializer _eventSerializer;
        
        static EventsRepositoryTests()
        {
            _eventSerializer = new JsonEventSerializer(new[] {typeof(DummyAggregate).Assembly});
        }

        public EventsRepositoryTests(CosmosFixture fixture)
        {
            _fixture = fixture;
        }


        [Fact]
        public async Task AppendAsync_should_store_events()
        {
            var db = await _fixture.CreateTestDatabaseAsync();

            var sut = new EventsRepository<DummyAggregate, Guid>(db.Client, db.Id, _eventSerializer);

            var aggregate = new DummyAggregate(Guid.NewGuid());
            aggregate.DoSomething("foo");
            aggregate.DoSomething("bar");

            await sut.AppendAsync(aggregate);

            var container = db.GetContainer("Events");
            var response = await container.GetItemLinqQueryable<dynamic>()
                                          .CountAsync();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Resource.Should().Be(3);
        }

        [Fact]
        public async Task AppendAsync_should_throw_AggregateException_when_version_mismatch()
        {
            var db = await _fixture.CreateTestDatabaseAsync();

            var sut = new EventsRepository<DummyAggregate, Guid>(db.Client, db.Id, _eventSerializer);

            var aggregateId = Guid.NewGuid();

            var tasks = Enumerable.Range(1, 3)
                .Select(i =>
                {
                    var aggregate = new DummyAggregate(aggregateId);
                    aggregate.DoSomething($"foo|{i}");
                    return sut.AppendAsync(aggregate);
                }).ToArray();

            await Assert.ThrowsAsync<AggregateException>(async () => await Task.WhenAll(tasks));
        }

        [Fact]
        public async Task RehydrateAsync_should_store_events()
        {
            var db = await _fixture.CreateTestDatabaseAsync();

            var sut = new EventsRepository<DummyAggregate, Guid>(db.Client, db.Id, _eventSerializer);

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
