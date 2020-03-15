using System.Threading.Tasks;

namespace SuperSafeBank.Console
{ 
    public class Program
    {
        static async Task Main(string[] args)
        {
            //var kafkaConnString = "localhost:9092";
            //var eventsTopic = "events";
            //var mongoConnString = "mongodb://root:password@localhost:27017";

            //var jsonEventDeserializer = new JsonEventDeserializer(new[]
            //{
            //    typeof(AccountCreated).Assembly
            //});

            //BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));

            //var serviceProvider = new ServiceCollection()
            //    .AddSingleton(new MongoClient(connectionString: mongoConnString))
            //    .AddSingleton(ctx =>
            //    {
            //        var client = ctx.GetRequiredService<MongoClient>();
            //        return client.GetDatabase("bankAccounts");
            //    })
            //    .AddLogging(cfg =>
            //    {
            //        cfg.ClearProviders()
            //            .AddConsole();
            //    })
            //    .AddMediatR(new[]
            //    {
            //        typeof(AccountEventsHandler).Assembly
            //    })
            //    .BuildServiceProvider();

            //var mediator = serviceProvider.GetRequiredService<IMediator>();

            ////var tc = Read(eventsTopic, kafkaConnString, jsonEventDeserializer, mediator);

            ////var tp = Write(eventsTopic, kafkaConnString, jsonEventDeserializer);

            ////await Task.WhenAll(tp, tc);

            //await Read(eventsTopic, kafkaConnString, jsonEventDeserializer, mediator);

            //System.Console.WriteLine("done!");
            // System.Console.ReadLine();
        }

        //private static Task Read(string eventsTopic, 
        //    string kafkaConnString,
        //    JsonEventDeserializer jsonEventDeserializer,
        //    IMediator mediator)
        //{
        //    var cts = new CancellationTokenSource();

        //    System.Console.CancelKeyPress += (s, e) =>
        //    {
        //        System.Console.WriteLine("shutting down...");
        //        e.Cancel = true;
        //        cts.Cancel();
        //    };

        //    var consumers = new List<Task>();
        //    consumers.AddRange(Enumerable.Range(1, 10)
        //        .Select(i => InitEventConsumer<Account, Guid>(eventsTopic, kafkaConnString, jsonEventDeserializer,
        //            mediator, i, cts.Token)));
        //    consumers.AddRange(Enumerable.Range(1, 10)
        //        .Select(i => InitEventConsumer<Customer, Guid>(eventsTopic, kafkaConnString, jsonEventDeserializer, mediator, i, cts.Token)));
            
        //    var tc = Task.WhenAll(consumers);
        //    return tc;
        //}

        //private static Task InitEventConsumer<TA, TK>(string eventsTopic, string kafkaConnString,
        //    JsonEventDeserializer jsonEventDeserializer, IMediator mediator, int consumerId, CancellationToken cancellationToken)
        //    where TA : IAggregateRoot<TK>
        //{
        //    var consumer = new EventConsumer<TA, TK>(eventsTopic, kafkaConnString, jsonEventDeserializer);

        //    async Task OnConsumerOnEventReceived(object s, IDomainEvent<TK> e)
        //    {
        //        var @event = EventReceivedFactory.Create((dynamic) e);

        //        System.Console.WriteLine($"consumer {consumerId} received event {@event.GetType()} for aggregate {e.AggregateId} , version {e.AggregateVersion}");

        //        await mediator.Publish(@event, cancellationToken);
        //    }

        //    consumer.EventReceived += OnConsumerOnEventReceived;

        //    return consumer.ConsumeAsync(cancellationToken);
        //}

        //private static async Task Write(string eventsTopic, 
        //    string kafkaConnString,
        //    JsonEventDeserializer jsonEventDeserializer)
        //{
        //    var eventStoreConnStr = new Uri("tcp://admin:changeit@localhost:1113");
        //    var connectionWrapper = new EventStoreConnectionWrapper(eventStoreConnStr);

        //    var customerEventsRepository = new EventsRepository<Customer, Guid>(connectionWrapper, jsonEventDeserializer);
        //    var customerEventsProducer = new EventProducer<Customer, Guid>(eventsTopic, kafkaConnString);

        //    var accountEventsRepository = new EventsRepository<Account, Guid>(connectionWrapper, jsonEventDeserializer);
        //    var accountEventsProducer = new EventProducer<Account, Guid>(eventsTopic, kafkaConnString);

        //    var customerEventsService = new EventsService<Customer, Guid>(customerEventsRepository, customerEventsProducer);
        //    var accountEventsService = new EventsService<Account, Guid>(accountEventsRepository, accountEventsProducer);

        //    var currencyConverter = new FakeCurrencyConverter();

        //    var customer = Customer.Create("lorem", "ipsum");
        //    await customerEventsService.PersistAsync(customer);

        //    var accounts = Enumerable.Range(1, 100)
        //        .Select(i =>
        //        {
        //            var account = Account.Create(customer, Currency.CanadianDollar);
        //            account.Deposit(new Money(Currency.CanadianDollar, 10), currencyConverter);
        //            account.Deposit(new Money(Currency.CanadianDollar, 42), currencyConverter);
        //            account.Withdraw(new Money(Currency.CanadianDollar, 4), currencyConverter);
        //            account.Deposit(new Money(Currency.CanadianDollar, 71), currencyConverter);
        //            return account;
        //        });
        //    await Task.WhenAll(accounts.Select(a => accountEventsService.PersistAsync(a)));
        //}
    }
}
