using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace SuperSafeBank.Persistence.EventStore.Tests
{

    public class EventStoreConnectionWrapperTests
    {
        [Fact]
        public async Task GetConnectionAsync_should_return_connection()
        {
            var connStr = new Uri(Settings.EventStoreConnectionString);
            var logger = NSubstitute.Substitute.For<ILogger<EventStoreConnectionWrapper>>();
            using var sut = new EventStoreConnectionWrapper(connStr, logger);

            var conn = await sut.GetConnectionAsync();
            conn.Should().NotBeNull();
        }
    }
}
