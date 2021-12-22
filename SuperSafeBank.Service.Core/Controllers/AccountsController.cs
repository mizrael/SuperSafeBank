using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.Commands;
using SuperSafeBank.Service.Core.Common.Queries;
using SuperSafeBank.Service.Core.DTOs;

namespace SuperSafeBank.Service.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AccountsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet, Route("{id:guid}", Name = "GetAccount")]
        public async Task<IActionResult> GetAccount(Guid id, CancellationToken cancellationToken = default)
        {
            var query = new AccountById(id);
            var result = await _mediator.Send(query, cancellationToken);
            if (null == result)
                return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto dto, CancellationToken cancellationToken = default)
        {
            if (null == dto)
                return BadRequest();

            var currency = Currency.FromCode(dto.CurrencyCode);
            var command = new CreateAccount(dto.CustomerId, Guid.NewGuid(), currency);
            await _mediator.Publish(command, cancellationToken);
            return CreatedAtAction("GetAccount", "Accounts", new { id = command.AccountId }, command);
        }


        [HttpPut, Route("{id:guid}/deposit")]
        public async Task<IActionResult> Deposit([FromRoute]Guid id, [FromBody]DepositDto dto, CancellationToken cancellationToken = default)
        {
            if (null == dto)
                return BadRequest();

            var currency = Currency.FromCode(dto.CurrencyCode);
            var amount = new Money(currency, dto.Amount);
            var command = new Deposit(id, amount);
            await _mediator.Publish(command, cancellationToken);
            return Ok();
        }

        [HttpPut, Route("{id:guid}/withdraw")]
        public async Task<IActionResult> Withdraw([FromRoute]Guid id, [FromBody]WithdrawDto dto, CancellationToken cancellationToken = default)
        {
            if (null == dto)
                return BadRequest();

            var currency = Currency.FromCode(dto.CurrencyCode);
            var amount = new Money(currency, dto.Amount);
            var command = new Withdraw(id, amount);
            await _mediator.Publish(command, cancellationToken);
            return Ok();
        }
    }
}