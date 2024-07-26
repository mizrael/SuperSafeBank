namespace SuperSafeBank.Domain.Services
{
    public class FakeCurrencyConverter : ICurrencyConverter 
    {
        public Money Convert(Money amount, Currency currency) => 
            amount.Currency == currency ? amount : new Money(currency, amount.Value);
    }
}