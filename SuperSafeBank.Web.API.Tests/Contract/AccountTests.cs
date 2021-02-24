using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using SuperSafeBank.Web.API.Tests.Fixtures;
using Xunit;

namespace SuperSafeBank.Web.API.Tests.Contract
{
    public class AccountTests : IClassFixture<WebApiFixture<Startup>>
    {
        private readonly WebApiFixture<Startup> _fixture;

        public AccountTests(WebApiFixture<Startup> fixture)
        {
            _fixture = fixture;
        }

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
        public async Task GetDetails_should_return_account_details()
        {
            var expectedAccount = new Core.Queries.Models.AccountDetails(Guid.NewGuid(), Guid.NewGuid(),
                "John", "Doe", new Domain.Money(Domain.Currency.CanadianDollar, 42));

            await _fixture.QueryModelsSeeder.CreateAccountDetails(expectedAccount);

            var endpoint = $"accounts/{expectedAccount.Id}";
            var response = await _fixture.HttpClient.GetAsync(endpoint);
            response.Should().NotBeNull();
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var account = await response.Content.ReadAsAsync<Core.Queries.Models.AccountDetails>();
            account.Should().NotBeNull();
            account.Id.Should().Be(expectedAccount.Id);
            account.OwnerFirstName.Should().Be(expectedAccount.OwnerFirstName);
            account.OwnerLastName.Should().Be(expectedAccount.OwnerLastName);
            account.OwnerId.Should().Be(expectedAccount.OwnerId);
            account.Balance.Should().Be(expectedAccount.Balance);
        }

        [Fact]
        public async Task Post_should_not_create_account_if_customer_invalid()
        {
            var customerId = Guid.NewGuid();

            var createAccountPayload = new
            {
                currencyCode = "cad"
            };
            var response = await _fixture.HttpClient.PostAsJsonAsync($"customers/{customerId}/accounts", createAccountPayload);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_should_create_account()
        {
            var createCustomerPayload = new
            {
                firstname = "Customer",
                lastname = "WithAccount",
                email = "customer-with-new-account@test.com"
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
        }

        [Fact]
        public async Task Should_not_be_able_to_deposit_funds_when_account_does_not_exists()
        {
            var accountId = Guid.NewGuid();
            var depositPayload = new
            {
                currencyCode = "cad",
                amount = 42
            };
            var response = await _fixture.HttpClient.PutAsJsonAsync($"accounts/{accountId}/deposit", depositPayload);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Should_be_able_to_deposit_funds_when_account_exists()
        {
            var createCustomerPayload = new
            {
                firstname = "Customer",
                lastname = "WithAccount",
                email = "customer-with-account@test.com"
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
        }

        [Fact]
        public async Task Should_not_be_able_to_withdraw_when_funds_not_available()
        {
            var createCustomerPayload = new
            {
                firstname = "Customer",
                lastname = "WithAccount",
                email = "not-enough-funds@test.com"
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

            var withdrawPayload = new
            {
                currencyCode = "cad",
                amount = 42
            };
            response = await _fixture.HttpClient.PutAsJsonAsync($"accounts/{accountId}/withdraw", withdrawPayload);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Should_be_able_to_withdraw_when_funds_available()
        {
            var createCustomerPayload = new
            {
                firstname = "Customer",
                lastname = "WithAccount",
                email = "enough-funds@test.com"
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
                amount = 71
            };
            response = await _fixture.HttpClient.PutAsJsonAsync($"accounts/{accountId}/deposit", depositPayload);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var withdrawPayload = new
            {
                currencyCode = "cad",
                amount = 42
            };
            response = await _fixture.HttpClient.PutAsJsonAsync($"accounts/{accountId}/withdraw", withdrawPayload);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}