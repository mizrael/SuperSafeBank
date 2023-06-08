using System;

namespace SuperSafeBank.Service.Core.Azure.DTOs
{
    public record CreateAccountDto(Guid CustomerId, string CurrencyCode);

    public record DepositDto(string CurrencyCode, decimal Amount);

    public record WithdrawDto(string CurrencyCode, decimal Amount);
}
