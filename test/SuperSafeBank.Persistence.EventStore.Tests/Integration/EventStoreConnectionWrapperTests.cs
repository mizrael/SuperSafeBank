using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace SuperSafeBank.Persistence.EventStore.Tests.Integration
{

    [Trait("Category", "Integration")]
    [Category("Integration")]
    public class EventStoreConnectionWrapperTests : IClassFixture<Fixtures.EventStoreFixture>
    {
        private readonly Fixtures.EventStoreFixture _fixture;

        public EventStoreConnectionWrapperTests(Fixtures.EventStoreFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetConnectionAsync_should_return_connection()
        {
            var connStr = new Uri(_fixture.ConnectionString);
            var logger = NSubstitute.Substitute.For<ILogger<EventStoreConnectionWrapper>>();
            using var sut = new EventStoreConnectionWrapper(connStr, logger);

            var conn = await sut.GetConnectionAsync();
            conn.Should().NotBeNull();
        }
    }
}
