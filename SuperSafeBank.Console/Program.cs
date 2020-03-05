using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using SuperSafeBank.Core;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.Events;
using SuperSafeBank.Domain.Services;
using SuperSafeBank.Persistence.EventStore;
using SuperSafeBank.Persistence.Kafka;

namespace SuperSafeBank.Console
{ 
    public class Program
    {
        static async Task Main(string[] args)
        {
            var kafkaConnString = "localhost:9092";
            var eventsTopic = "events";
            var mongoConnString = "mongodb://root:password@localhost:27017";

            var jsonEventDeserializer = new JsonEventDeserializer(new[]
            {
                typeof(AccountCreated).Assembly
            });

            BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));

            var serviceProvider = new ServiceCollection()
                .AddSingleton(new MongoClient(connectionString: mongoConnString))
                .AddSingleton(ctx =>
                {
                    var client = ctx.GetRequiredService<MongoClient>();
                    return client.GetDatabase("bankAccounts");
                })
                .AddLogging(cfg =>
                {
                    cfg.ClearProviders()
                        .AddConsole();
                })
                .AddMediatR(new[]
                {
                    typeof(AccountEventsHandler).Assembly
                })
                .BuildServiceProvider();

            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var tc = Read(eventsTopic, kafkaConnString, jsonEventDeserializer, mediator);

            var tp = Write(eventsTopic, kafkaConnString, jsonEventDeserializer);

            await Task.WhenAll(tp, tc);

            System.Console.WriteLine("done!");
            System.Console.ReadLine();
        }

        private static Task Read(string eventsTopic, 
            string kafkaConnString,
            JsonEventDeserializer jsonEventDeserializer,
            IMediator mediator)
        {
            var cts = new CancellationTokenSource();

            System.Console.CancelKeyPress += (s, e) =>
            {
                System.Console.WriteLine("shutting down...");
                e.Cancel = true;
                cts.Cancel();
            };

            var consumer = new EventConsumer<Account, Guid>(eventsTopic, kafkaConnString, jsonEventDeserializer);
            consumer.EventReceived += async (s, e) =>
            {
                var @event = EventReceivedFactory.Create((dynamic)e);
                await mediator.Publish(@event, cts.Token);
            };

            var tc = consumer.ConsumeAsync(cts.Token);
            return tc;
        }

        private static async Task Write(string eventsTopic, 
            string kafkaConnString,
            JsonEventDeserializer jsonEventDeserializer)
        {
            var eventStoreConnStr = new Uri("tcp://admin:changeit@localhost:1113");
            var connectionWrapper = new EventStoreConnectionWrapper(eventStoreConnStr);

            var customerEventsRepository = new EventsRepository<Customer, Guid>(connectionWrapper, jsonEventDeserializer);
            var customerEventsProducer = new EventProducer<Customer, Guid>(eventsTopic, kafkaConnString);

            var accountEventsRepository = new EventsRepository<Account, Guid>(connectionWrapper, jsonEventDeserializer);
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
