using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SuperSafeBank.Core.Models;
using System.Threading.Tasks;
using Confluent.Kafka;
using SuperSafeBank.Console.EventBus;
using SuperSafeBank.Core.Services;

namespace SuperSafeBank.Console
{ 
    public class Program
    {
        static async Task Main(string[] args)
        {
            var kafkaConnString = "localhost:9092";
            var eventsTopic = "events";

            var accounts = await Produce(eventsTopic, kafkaConnString);
            
            System.Console.WriteLine("done!");
        }

        private static async Task<IEnumerable<Account>> Produce(string eventsTopic, string kafkaConnString)
        {
            var customerEventsRepo = new EventProducer<Customer, Guid>(eventsTopic, kafkaConnString);
            var accountEventsRepo = new EventProducer<Account, Guid>(eventsTopic, kafkaConnString);

            var currencyConverter = new FakeCurrencyConverter();

            var accounts = new List<Account>();

            for (var i = 0; i != 10; ++i)
            {
                var customer = Customer.Create(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
                await customerEventsRepo.DispatchAsync(customer);

                var account = Account.Create(customer, Currency.CanadianDollar);
                account.Deposit(new Money(Currency.CanadianDollar, 10), currencyConverter);
                account.Deposit(new Money(Currency.CanadianDollar, 42), currencyConverter);
                account.Withdraw(new Money(Currency.CanadianDollar, 4), currencyConverter);
                account.Deposit(new Money(Currency.CanadianDollar, 71), currencyConverter);

                await accountEventsRepo.DispatchAsync(account);

                accounts.Add(account);
            }

            return accounts;
        }
    }
}
