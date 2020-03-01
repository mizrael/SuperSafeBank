using SuperSafeBank.Core.Models;
using System.Threading.Tasks;
using Confluent.Kafka;
using Marten;
using SuperSafeBank.Core.Services;

namespace SuperSafeBank.Console
{
    partial class Program
    {
        static async Task Main(string[] args)
        {
            var kafkaConnString = "127.0.0.1:9092";
            var eventsTopic = "events";

            var config = new ProducerConfig {BootstrapServers = kafkaConnString};
            var repository = new EventsRepository(eventsTopic, config);

            var currencyConverter = new FakeCurrencyConverter();

            var customer = Customer.Create("lorem", "ipsum");

            var account = Account.Create(customer, Currency.CanadianDollar);
            account.Deposit(new Money(Currency.CanadianDollar, 10), currencyConverter);
            account.Deposit(new Money(Currency.CanadianDollar, 42), currencyConverter);
            account.Withdraw(new Money(Currency.CanadianDollar, 4), currencyConverter);
            account.Deposit(new Money(Currency.CanadianDollar, 71), currencyConverter);

            await repository.AppendAsync(customer.Events);
            customer.ClearEvents();

            await repository.AppendAsync(account.Events);
            account.ClearEvents();

            System.Console.WriteLine("Hello World!");
        }
    }
}
