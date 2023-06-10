using SuperSafeBank.Service.Core.Persistence.Mongo;
using SuperSafeBank.Service.Core.Persistence.Mongo.Tests.Integration;
using System.ComponentModel;

namespace SuperSafeBank.Persistence.Mongo.Tests.Integration
{
    [Trait("Category", "Integration")]
    [Category("Integration")]
    public class CustomerEmailsServiceTests : IClassFixture<MongoFixture>
    {
        private readonly MongoFixture _fixture;

        public CustomerEmailsServiceTests(MongoFixture fixture)
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
