using System;
using System.Threading;
using System.Threading.Tasks;
using SuperSafeBank.Console.EventBus;
using SuperSafeBank.Core;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.Services;
using SuperSafeBank.Persistence.EventStore;

namespace SuperSafeBank.Console
{ 
    public class Program
    {
        static async Task Main(string[] args)
        {
            var kafkaConnString = "localhost:9092";
            var eventsTopic = "events";

            var cts = new CancellationTokenSource();

            System.Console.CancelKeyPress += (s, e) =>
            {
                System.Console.WriteLine("shutting down...");
                e.Cancel = true;
                cts.Cancel();
            };

            var consumer = new EventConsumer<Account, Guid>(eventsTopic, kafkaConnString);
            var tc = consumer.ConsumeAsync(cts.Token);

            var tp = Write(eventsTopic, kafkaConnString);

            await Task.WhenAll(tp, tc);

            System.Console.WriteLine("done!");
            System.Console.ReadLine();
        }

        private static async Task Write(string eventsTopic, string kafkaConnString)
        {
            var eventStoreConnStr = new Uri("tcp://admin:changeit@localhost:1113");
            var connectionWrapper = new EventStoreConnectionWrapper(eventStoreConnStr);

            var customerEventsRepository = new EventsRepository<Customer, Guid>(connectionWrapper);
            var customerEventsProducer = new EventProducer<Customer, Guid>(eventsTopic, kafkaConnString);

            var accountEventsRepository = new EventsRepository<Account, Guid>(connectionWrapper);
            var accountEventsProducer = new EventProducer<Account, Guid>(eventsTopic, kafkaConnString);

            var customerEventsService = new EventsService<Customer, Guid>(customerEventsRepository, customerEventsProducer);
            var accountEventsService = new EventsService<Account, Guid>(accountEventsRepository, accountEventsProducer);

            var currencyConverter = new FakeCurrencyConverter();

            var customer = Customer.Create("lorem", "ipsum");
            await customerEventsService.PersistAsync(customer);

            var account = Account.Create(customer, Currency.CanadianDollar);
            account.Deposit(new Money(Currency.CanadianDollar, 10), currencyConverter);
            account.Deposit(new Money(Currency.CanadianDollar, 42), currencyConverter);
            account.Withdraw(new Money(Currency.CanadianDollar, 4), currencyConverter);
            account.Deposit(new Money(Currency.CanadianDollar, 71), currencyConverter);
            await accountEventsService.PersistAsync(account);

            account.Withdraw(new Money(Currency.CanadianDollar, 10), currencyConverter);
            account.Deposit(new Money(Currency.CanadianDollar, 11), currencyConverter);
            await accountEventsService.PersistAsync(account);
        }
    }
}
