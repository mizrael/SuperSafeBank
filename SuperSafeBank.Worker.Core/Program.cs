using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using SuperSafeBank.Common;
using SuperSafeBank.Common.EventBus;
using SuperSafeBank.Domain.DomainEvents;
using SuperSafeBank.Service.Core.Common;
using SuperSafeBank.Service.Core.Common.EventHandlers;
using SuperSafeBank.Service.Core.Persistence.Mongo;
using SuperSafeBank.Service.Core.Persistence.Mongo.EventHandlers;
using SuperSafeBank.Transport.Kafka;
using SuperSafeBank.Worker.Core;
using SuperSafeBank.Persistence.EventStore;
using SuperSafeBank.Domain.Services;

await Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(configurationBuilder =>
    {
        configurationBuilder.AddCommandLine(args);
    })
    .ConfigureAppConfiguration((ctx, builder) =>
    {
        builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
    })
    .UseSerilog((ctx, cfg) =>
    {
        cfg.Enrich.FromLogContext()
            .Enrich.WithProperty("Application", ctx.HostingEnvironment.ApplicationName)
            .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName)
            .WriteTo.Console();

        var connStr = ctx.Configuration.GetConnectionString("loki");
        cfg.WriteTo.GrafanaLoki(connStr);
    })
    .ConfigureServices((hostContext, services) =>
    {
        var kafkaConnStr = hostContext.Configuration.GetConnectionString("kafka");
        var eventsTopicName = hostContext.Configuration["eventsTopicName"];
        var groupName = hostContext.Configuration["eventsTopicGroupName"];
        var consumerConfig = new EventsConsumerConfig(kafkaConnStr, eventsTopicName, groupName);

        var mongoConnStr = hostContext.Configuration.GetConnectionString("mongo");
        var mongoQueryDbName = hostContext.Configuration["queryDbName"];
        var mongoConfig = new MongoConfig(mongoConnStr, mongoQueryDbName);

        var eventstoreConnStr = hostContext.Configuration.GetConnectionString("eventstore");

        services.Scan(scan =>
        {
            scan.FromAssembliesOf(typeof(AccountEventsHandler))                
                .RegisterHandlers(typeof(INotificationHandler<>));
        }).Decorate(typeof(INotificationHandler<>), typeof(RetryDecorator<>))
            .AddTransient<ICurrencyConverter, FakeCurrencyConverter>()
            .AddScoped<ServiceFactory>(ctx => ctx.GetRequiredService)
            .AddScoped<IMediator, Mediator>()
            .AddSingleton<IEventSerializer>(new JsonEventSerializer(new[]
            {
                typeof(CustomerEvents.CustomerCreated).Assembly
            }))
            .AddSingleton(consumerConfig)
            .AddSingleton(typeof(IEventConsumer), typeof(EventConsumer))           
            .AddMongoDb(mongoConfig)
            .AddEventStore(eventstoreConnStr)
            .RegisterWorker();
    })
    .Build()
    .RunAsync();
    