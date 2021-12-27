using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using SuperSafeBank.Common;
using SuperSafeBank.Domain.Commands;
using SuperSafeBank.Service.Core.Azure.DTOs;
using SuperSafeBank.Service.Core.Common.Queries;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace SuperSafeBank.Service.Core.Azure.Triggers
{
    public class CustomerTriggers
    {
        private readonly IMediator _mediator;

        public CustomerTriggers(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        [Function(nameof(GetCustomers))]
        public async Task<HttpResponseData> GetCustomers([HttpTrigger(AuthorizationLevel.Function, "get", Route = "customers")] HttpRequestData req)
        {
            var query = new CustomersArchive();
            var results = await _mediator.Send(query);
            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteAsJsonAsync(results);
            return response;
        }

        [Function(nameof(CreateCustomer))]
        public async Task<HttpResponseData> CreateCustomer([HttpTrigger(AuthorizationLevel.Function, "post", Route = "customers")] HttpRequestData req)
        {
            var dto = await JsonSerializer.DeserializeAsync<CreateCustomerDto>(req.Body, JsonSerializerDefaultOptions.Defaults);
            var command = new CreateCustomer(Guid.NewGuid(), dto.FirstName, dto.LastName, dto.Email);
            await _mediator.Publish(command);

            var response = req.CreateResponse(System.Net.HttpStatusCode.Created);
            response.Headers.Add("Location", $"/customers/{command.CustomerId}");
            await response.WriteAsJsonAsync(new { id = command.CustomerId });

            return response;
        }

        [Function(nameof(GetCustomerById))]
        public async Task<HttpResponseData> GetCustomerById([HttpTrigger(AuthorizationLevel.Function, "get", Route = "customers/{customerId:guid}")] HttpRequestData req,
            Guid customerId)
        {
            var query = new CustomerById(customerId);
            var result = await _mediator.Send(query);
            if (result is null)
                return req.CreateResponse(System.Net.HttpStatusCode.NotFound);

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);
            return response;
        }
    }
}
