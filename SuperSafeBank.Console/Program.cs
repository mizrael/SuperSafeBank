using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SuperSafeBank.Core.Models;
using System.Threading.Tasks;
using Confluent.Kafka;
using SuperSafeBank.Core.Services;

namespace SuperSafeBank.Console
{ 
    public class Program
    {
        static async Task Main(string[] args)
        {
            var kafkaConnString = "localhost:9092";
            var eventsTopic = "events";

            var accounts = await Produce(kafkaConnString, eventsTopic);

            Consume<Account, Guid>(kafkaConnString, eventsTopic);

            System.Console.WriteLine("done!");
        }

        private static void Consume<TA, TKey>(string kafkaConnString, string topicBaseName)
            where TA : IAggregateRoot<TKey>
        {
            var keyDeserializerFactory = new KeyDeserializerFactory();

            var conf = new ConsumerConfig
            {
                GroupId = "test-consumer-group",
                BootstrapServers = kafkaConnString,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnablePartitionEof = true
            };

            var builder = new ConsumerBuilder<Guid, string>(conf);
            builder.SetKeyDeserializer(keyDeserializerFactory.Create<Guid>());

            using var c = builder.Build();

            var aggregateType = typeof(TA);
            var topicName = $"{topicBaseName}-{aggregateType.Name}";
            c.Subscribe(topicName);

            var cts = new CancellationTokenSource();
            System.Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            try
            {
                while (true)
                {
                    try
                    {
                        var cr = c.Consume(cts.Token);
                        if (cr.IsPartitionEOF)
                            continue;

                        var messageTypeHeader = cr.Headers.First(h => h.Key == "type");
                        var messageType = Encoding.UTF8.GetString(messageTypeHeader.GetValueBytes());

                        System.Console.WriteLine($"Consumed '{messageType}' message: '{cr.Key}' -> '{cr.Value}' at: '{cr.TopicPartitionOffset}'.");
                    }
                    catch (ConsumeException e)
                    {
                        System.Console.WriteLine($"Error occured: {e.Error.Reason}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ensure the consumer leaves the group cleanly and final offsets are committed.
                c.Close();
            }
        }

        private static async Task<IEnumerable<Account>> Produce(string kafkaConnString, string eventsTopic)
        {
            var config = new ProducerConfig {BootstrapServers = kafkaConnString};

            var customerEventsRepo = new EventsRepository<Customer, Guid>(eventsTopic, config);
            var accountEventsRepo = new EventsRepository<Account, Guid>(eventsTopic, config);

            var currencyConverter = new FakeCurrencyConverter();

            var accounts = new List<Account>();

            for (var i = 0; i != 10; ++i)
            {
                var customer = Customer.Create(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
                await customerEventsRepo.AppendAsync(customer);

                var account = Account.Create(customer, Currency.CanadianDollar);
                account.Deposit(new Money(Currency.CanadianDollar, 10), currencyConverter);
                account.Deposit(new Money(Currency.CanadianDollar, 42), currencyConverter);
                account.Withdraw(new Money(Currency.CanadianDollar, 4), currencyConverter);
                account.Deposit(new Money(Currency.CanadianDollar, 71), currencyConverter);
    
                await accountEventsRepo.AppendAsync(account);

                accounts.Add(account);
            }

            return accounts;
        }
    }


}
