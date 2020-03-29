using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using SuperSafeBank.Web.API.Tests.Fixtures;
using Xunit;

namespace SuperSafeBank.Web.API.Tests.Contract
{
    public class CustomerTests : IClassFixture<WebApiFixture<Startup>>
    {
        private readonly WebApiFixture<Startup> _fixture;

        public CustomerTests(WebApiFixture<Startup> fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetDetails_should_return_404_if_id_invalid()
        {
            var endpoint = $"customers/{Guid.NewGuid()}";
            var response = await _fixture.HttpClient.GetAsync(endpoint);
            response.Should().NotBeNull();
            response.IsSuccessStatusCode.Should().BeFalse();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_should_create_customer()
        {
            var payload = new
            {
                firstname = Guid.NewGuid(),
                lastname = Guid.NewGuid()
            };
            var response = await _fixture.HttpClient.PostAsJsonAsync("customers", payload);
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();

            await TestUtils.Retry(async () =>
            {
                var detailsResponse = await _fixture.HttpClient.GetAsync(response.Headers.Location);
                detailsResponse.IsSuccessStatusCode.Should().BeTrue();

                var details = await detailsResponse.Content.ReadAsAsync<dynamic>();
                
                string firstName = details.firstname;
                payload.firstname.Should().Be(firstName);

                string lastName = details.lastname;
                payload.lastname.Should().Be(lastName);

                return true;
            }, "failed to fetch customer by id");
        }
    }
}
