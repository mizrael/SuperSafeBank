using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using SuperSafeBank.Domain.Commands;
using SuperSafeBank.Service.Core.Azure.DTOs;
using SuperSafeBank.Service.Core.Common.Queries;
using System;
using System.Threading;
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
        public async Task<HttpResponseData> GetCustomers([HttpTrigger(AuthorizationLevel.Function, "get", Route = "customers")] HttpRequestData req,             
            CancellationToken cancellationToken = default)
        {            
            var query = new CustomersArchive();
            var results = await _mediator.Send(query, cancellationToken);            
            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteAsJsonAsync(results);
            return response;
        }

        [Function(nameof(CreateCustomer))]
        public async Task<HttpResponseData> CreateCustomer([HttpTrigger(AuthorizationLevel.Function, "post", Route = "customers")] HttpRequestData req,            
            CancellationToken cancellationToken = default)
        {
            var dto = await System.Text.Json.JsonSerializer.DeserializeAsync<CreateCustomerDto>(req.Body);
            var command = new CreateCustomer(Guid.NewGuid(), dto.FirstName, dto.LastName, dto.Email);
            await _mediator.Publish(command, cancellationToken);

            var response = req.CreateResponse(System.Net.HttpStatusCode.Created);
            response.Headers.Add("Location", $"/customers/{command.Id}");
            await response.WriteAsJsonAsync(new { id = command.Id }, cancellationToken);

            return response;
        }

        [Function(nameof(GetCustomerById))]
        public async Task<HttpResponseData> GetCustomerById([HttpTrigger(AuthorizationLevel.Function, "get", Route = "customers/{customerId:guid}")] HttpRequestData req,
            Guid customerId,
            CancellationToken cancellationToken = default)
        {
            var query = new CustomerById(customerId);
            var result = await _mediator.Send(query, cancellationToken);
            if (result is null)
                return req.CreateResponse(System.Net.HttpStatusCode.NotFound);

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);
            return response;
        }
    }
}
