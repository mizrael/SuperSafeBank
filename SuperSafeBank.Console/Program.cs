using System;
using System.Threading.Tasks;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.Services;
using SuperSafeBank.Persistence.EventStore;

namespace SuperSafeBank.Console
{ 
    public class Program
    {
        static async Task Main(string[] args)
        {
            var eventStoreConnStr = new Uri("tcp://admin:changeit@localhost:1113");
            var connectionWrapper = new EventStoreConnectionWrapper(eventStoreConnStr);

            var customerEventsRepository = new EventsRepository<Customer, Guid>(connectionWrapper);
            var accountEventsRepository = new EventsRepository<Account, Guid>(connectionWrapper);

            var currencyConverter = new FakeCurrencyConverter();

            var customer = Customer.Create("lorem", "ipsum");
            await customerEventsRepository.AppendAsync(customer);

            var account = Account.Create(customer, Currency.CanadianDollar);
            account.Deposit(new Money(Currency.CanadianDollar, 10), currencyConverter);
            account.Deposit(new Money(Currency.CanadianDollar, 42), currencyConverter);
            account.Withdraw(new Money(Currency.CanadianDollar, 4), currencyConverter);
            account.Deposit(new Money(Currency.CanadianDollar, 71), currencyConverter);
            await accountEventsRepository.AppendAsync(account);

            account.Withdraw(new Money(Currency.CanadianDollar, 10), currencyConverter);
            account.Deposit(new Money(Currency.CanadianDollar, 11), currencyConverter);
            await accountEventsRepository.AppendAsync(account);

            var rehydratedAccount = await accountEventsRepository.RehydrateAsync(account.Id);

            System.Console.WriteLine("done!");
        }
    }
}
