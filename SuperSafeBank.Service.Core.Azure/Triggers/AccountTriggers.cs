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
    public class AccountTriggers
    {
        private readonly IMediator _mediator;

        public AccountTriggers(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        [Function(nameof(GetAccount))]
        public async Task<HttpResponseData> GetAccount([HttpTrigger(AuthorizationLevel.Function, "get", Route = "accounts/{accountId:guid}")] HttpRequestData req, Guid accountId)
        {
            var query = new AccountById(accountId);
            var result = await _mediator.Send(query);
            if (result is null)
                return req.CreateResponse(System.Net.HttpStatusCode.NotFound);

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);
            return response;
        }

        [Function(nameof(CreateAccount))]
        public async Task<HttpResponseData> CreateAccount([HttpTrigger(AuthorizationLevel.Function, "post", Route = "accounts")] HttpRequestData req)
        {
            var dto = await JsonSerializer.DeserializeAsync<CreateAccountDto>(req.Body, JsonSerializerDefaultOptions.Defaults);
            var command = new CreateAccount(customerId: dto.CustomerId, accountId: Guid.NewGuid(), currency: Domain.Currency.FromCode(dto.CurrencyCode));
            await _mediator.Publish(command);

            var response = req.CreateResponse(System.Net.HttpStatusCode.Created);
            response.Headers.Add("Location", $"/accounts/{command.AccountId}");
            await response.WriteAsJsonAsync(new { id = command.AccountId });

            return response;
        }

        [Function(nameof(Deposit))]
        public async Task<HttpResponseData> Deposit([HttpTrigger(AuthorizationLevel.Function, "put", Route = "accounts/{accountId:guid}/deposit")] HttpRequestData req, Guid accountId)
        {
            var dto = await JsonSerializer.DeserializeAsync<DepositDto>(req.Body, JsonSerializerDefaultOptions.Defaults);

            var currency = Domain.Currency.FromCode(dto.CurrencyCode);
            var amount = new Domain.Money(currency, dto.Amount);
            var command = new Deposit(accountId, amount);

            await _mediator.Publish(command);

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            return response;
        }

        [Function(nameof(Withdraw))]
        public async Task<HttpResponseData> Withdraw([HttpTrigger(AuthorizationLevel.Function, "put", Route = "accounts/{accountId:guid}/withdraw")] HttpRequestData req, Guid accountId)
        {
            var dto = await JsonSerializer.DeserializeAsync<WithdrawDto>(req.Body, JsonSerializerDefaultOptions.Defaults);

            var currency = Domain.Currency.FromCode(dto.CurrencyCode);
            var amount = new Domain.Money(currency, dto.Amount);
            var command = new Withdraw(accountId, amount);

            await _mediator.Publish(command);

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            return response;
        }
    }
}
