using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace SuperSafeBank.Persistence.Mongo.Tests.Integration
{
    public class CustomerEmailsServiceTests : IClassFixture<Fixtures.MongoFixture>
    {
        private readonly Fixtures.MongoFixture _fixture;

        public CustomerEmailsServiceTests(Fixtures.MongoFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ExistsAsync_should_return_false_if_email_invalid()
        {
            var sut = new CustomerEmailsService(_fixture.Database);
            var result = await sut.ExistsAsync(Guid.NewGuid().ToString());
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsAsync_should_return_true_if_email_valid()
        {
            var sut = new CustomerEmailsService(_fixture.Database);

            var email = Guid.NewGuid().ToString();
            var customerId = Guid.NewGuid();
            await sut.CreateAsync(email, customerId);

            var result = await sut.ExistsAsync(email);
            result.Should().BeTrue();
        }
    }
}
