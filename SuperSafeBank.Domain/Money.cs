using System;

namespace SuperSafeBank.Domain
{
    public record Money
    {
        public Money(Currency currency, decimal value)
        {
            Value = value;
            Currency = currency ?? throw new ArgumentNullException(nameof(currency));
        }

        public decimal Value { get; }
        public Currency Currency { get; }

        public Money Subtract(decimal amount) => new Money(this.Currency, this.Value - amount);

        public Money Add(decimal amount) => new Money(this.Currency, this.Value + amount);

        public override string ToString() => $"{Value} {Currency}";

        public static Money Zero(Currency currency) => new Money(currency, 0);
    }
}