using SuperSafeBank.Core.Models;

namespace SuperSafeBank.Core.Services
{
    public interface ICurrencyConverter
    {
        Money Convert(Money amount, Currency currency);
    }
}