using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using SuperSafeBank.Common;
using SuperSafeBank.Common.EventBus;
using SuperSafeBank.Domain.DomainEvents;
using SuperSafeBank.Transport.Kafka;
using SuperSafeBank.Worker.Notifications;
using SuperSafeBank.Worker.Notifications.ApiClients;

await Host.CreateDefaultBuilder(args)
        .ConfigureHostConfiguration(configurationBuilder => {
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
            services.AddHttpClient<ICustomersApiClient, CustomersApiClient>("customersApiClient", (ctx, httpClient) =>
            {
                var config = ctx.GetRequiredService<IConfiguration>();
                var endpoint = config["CustomersApi"];
                httpClient.BaseAddress = new System.Uri(endpoint);
            }).AddPolicyHandler(HttpClientPolicies.GetRetryPolicy());

            services.AddHttpClient<IAccountsApiClient, AccountsApiClient>("accountsApiClient", (ctx, httpClient) =>
            {
                var config = ctx.GetRequiredService<IConfiguration>();
                var endpoint = config["AccountsApi"];
                httpClient.BaseAddress = new System.Uri(endpoint);
            }).AddPolicyHandler(HttpClientPolicies.GetRetryPolicy());

            services.AddSingleton<IEventSerializer>(new JsonEventSerializer(new[]
            {
                typeof(CustomerEvents.CustomerCreated).Assembly
            }))
            .AddSingleton<INotificationsFactory, NotificationsFactory>()
            .AddSingleton<INotificationsService, FakeNotificationsService>()
            .AddSingleton(ctx =>
            {
                var kafkaConnStr = hostContext.Configuration.GetConnectionString("kafka");
                var eventsTopicName = hostContext.Configuration["eventsTopicName"];
                var groupName = hostContext.Configuration["eventsTopicGroupName"];
                return new EventsConsumerConfig(kafkaConnStr, eventsTopicName, groupName);
            })
            .AddSingleton(typeof(IEventConsumer), typeof(KafkaEventConsumer))
            .AddHostedService(ctx =>
            {
                var logger = ctx.GetRequiredService<ILogger<AccountEventsWorker>>();
                var eventsDeserializer = ctx.GetRequiredService<IEventSerializer>();
                var consumerConfig = ctx.GetRequiredService<EventsConsumerConfig>();
                var notificationsFactory = ctx.GetRequiredService<INotificationsFactory>();
                var notificationsService = ctx.GetRequiredService<INotificationsService>();

                var consumer = ctx.GetRequiredService<IEventConsumer>();

                return new AccountEventsWorker(notificationsFactory, notificationsService, consumer, logger);
            });
        }).Build().RunAsync();