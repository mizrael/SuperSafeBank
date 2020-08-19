using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.Commands;
using SuperSafeBank.Web.API.DTOs;
using SuperSafeBank.Web.Core.Queries;

namespace SuperSafeBank.Web.API.Controllers
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