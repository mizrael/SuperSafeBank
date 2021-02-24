using System;
using SuperSafeBank.Core.Models;

namespace SuperSafeBank.Domain
{
    public class Money : ValueObject<Money>
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

        protected override int GetHashCodeCore() => HashCode.Combine(this.Value, this.Currency);

        protected override bool EqualsCore(Money other)
            => this.Value == other.Value &&
               this.Currency == other.Currency;

        public override string ToString() => $"{Value} {Currency}";

        public static Money Zero(Currency currency) => new Money(currency, 0);
    }
}