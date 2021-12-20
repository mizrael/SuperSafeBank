using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using SuperSafeBank.Service.Core.Tests.Fixtures;
using Xunit;

namespace SuperSafeBank.Service.Core.Tests.Contract
{
    [Trait("Category", "E2E")]
    [Category("E2E")]
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
        public async Task GetDetails_should_return_customer_details()
        {
            var customerDetails = new Common.Queries.CustomerDetails(Guid.NewGuid(),
                "test", "customer", "customer@details.com", null, Domain.Money.Zero(Domain.Currency.CanadianDollar));

            await _fixture.QueryModelsSeeder.CreateCustomerDetails(customerDetails);

            var endpoint = $"customers/{customerDetails.Id}";
            var response = await _fixture.HttpClient.GetAsync(endpoint);
            response.Should().NotBeNull();
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadAsAsync<Common.Queries.CustomerDetails>();
            
            result.Id.Should().Be(customerDetails.Id);
            result.Firstname.Should().Be(customerDetails.Firstname);            
            result.Lastname.Should().Be(customerDetails.Lastname);
            result.Email.Should().Be(customerDetails.Email);
            result.Accounts.Should().NotBeNull().And.BeEmpty();
            result.TotalBalance.Should().Be(customerDetails.TotalBalance);
        }

        [Fact]
        public async Task GetArchive_should_return_customers_archive()
        {
            var customerItem = new Common.Queries.CustomerArchiveItem(
                Guid.NewGuid(),
                "test", "customer", null);
            await _fixture.QueryModelsSeeder.CreateCustomerArchiveItem(customerItem);

            var endpoint = $"customers/";
            var response = await _fixture.HttpClient.GetAsync(endpoint);
            response.Should().NotBeNull();
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadAsAsync<Common.Queries.CustomerArchiveItem[]>();
            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);

            var customer = result.FirstOrDefault(c => c.Id == customerItem.Id);
            customer.Should().NotBeNull();

            customer.Id.Should().Be(customerItem.Id);
            customer.Firstname.Should().Be(customerItem.Firstname);
            customer.Lastname.Should().Be(customerItem.Lastname);
            customer.Accounts.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public async Task Post_should_create_customer()
        {
            var payload = new
            {
                firstname = "test customer",
                lastname = "creation",
                email = "test@test.com"
            };
            var response = await _fixture.HttpClient.PostAsJsonAsync("customers", payload);
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
        }

        [Fact]
        public async Task Post_should_not_create_customer_if_email_already_exists() 
        {
            var payload = new
            {
                firstname = "existing",
                lastname = "customer",
                email = "existing@customer.com"
            };
            var response = await _fixture.HttpClient.PostAsJsonAsync("customers", payload);
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();

            payload = new
            {
                firstname = "another",
                lastname = "customer",
                email = "existing@customer.com"
            };
            response = await _fixture.HttpClient.PostAsJsonAsync("customers", payload);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}