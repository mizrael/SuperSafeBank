using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using SuperSafeBank.Web.API.Tests.Fixtures;
using Xunit;

namespace SuperSafeBank.Web.API.Tests.Contract
{
    public class AccountTests
#if OnPremise
        : IClassFixture<OnPremiseWebApiFixture<Startup>>
#endif

    {
        private readonly BaseWebApiFixture<Startup> _fixture;

#if OnPremise
        public AccountTests(OnPremiseWebApiFixture<Startup> fixture)
        {
            _fixture = fixture;
        }
#endif
        [Fact]
        public async Task GetDetails_should_return_404_if_id_invalid()
        {
            var endpoint = $"accounts/{Guid.NewGuid()}";
            var response = await _fixture.HttpClient.GetAsync(endpoint);
            response.Should().NotBeNull();
            response.IsSuccessStatusCode.Should().BeFalse();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_should_create_account()
        {
            var createCustomerPayload = new
            {
                firstname = "Customer",
                lastname = "WithAccount",
                email = "test@test.com"
            };
            var response = await _fixture.HttpClient.PostAsJsonAsync("customers", createCustomerPayload);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadAsAsync<dynamic>();

            Guid customerId = result.id;

            var createAccountPayload = new
            {
                currencyCode = "cad"
            };
            response = await _fixture.HttpClient.PostAsJsonAsync($"customers/{customerId}/accounts", createAccountPayload);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            await TestUtils.Retry(async () =>
            {
                var detailsResponse = await _fixture.HttpClient.GetAsync(response.Headers.Location);
                detailsResponse.IsSuccessStatusCode.Should().BeTrue();

                var details = await detailsResponse.Content.ReadAsAsync<dynamic>();
                Guid accountCustomerId = details.ownerId;
                customerId.Should().Be(accountCustomerId);

                return true;
            }, "failed to fetch account by id");
        }

        [Fact]
        public async Task Should_be_able_to_deposit_funds()
        {
            var createCustomerPayload = new
            {
                firstname = "Customer",
                lastname = "WithAccount",
                email = "test@test.com"
            };
            var response = await _fixture.HttpClient.PostAsJsonAsync("customers", createCustomerPayload);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadAsAsync<dynamic>();

            Guid customerId = result.id;

            var createAccountPayload = new
            {
                currencyCode = "cad"
            };
            response = await _fixture.HttpClient.PostAsJsonAsync($"customers/{customerId}/accounts", createAccountPayload);
            var responseBody = await response.Content.ReadAsAsync<dynamic>();
            Guid accountId = responseBody.accountId;

            accountId.Should().NotBeEmpty();

            var depositPayload = new
            {
                currencyCode = "cad",
                amount = 42
            };
            response = await _fixture.HttpClient.PutAsJsonAsync($"accounts/{accountId}/deposit", depositPayload);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            await TestUtils.Retry(async () =>
            {
                var detailsResponse = await _fixture.HttpClient.GetAsync($"accounts/{accountId}");
                detailsResponse.IsSuccessStatusCode.Should().BeTrue();

                var details = await detailsResponse.Content.ReadAsAsync<dynamic>();
                decimal value = details.balance.value;
                value.Should().Be(depositPayload.amount);

                return true;
            }, "failed to fetch account by id");
        }

    }
}