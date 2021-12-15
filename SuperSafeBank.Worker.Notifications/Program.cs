using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Compact;
#if OnPremise
using Serilog.Sinks.Grafana.Loki;
#endif
using SuperSafeBank.Core;
using SuperSafeBank.Domain;
using SuperSafeBank.Domain.Events;
using SuperSafeBank.Persistence.Kafka;
using SuperSafeBank.Worker.Notifications.ApiClients;

namespace SuperSafeBank.Worker.Notifications
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
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
                    cfg.MinimumLevel.Verbose()
                        .Enrich.FromLogContext()
                        .Enrich.WithProperty("Application", ctx.HostingEnvironment.ApplicationName)
                        .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName)
                        .WriteTo.Console(new RenderedCompactJsonFormatter());

#if OnPremise
                    var connStr = ctx.Configuration.GetConnectionString("loki"); 
                    cfg.WriteTo.GrafanaLoki(connStr);
#endif

                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IEventSerializer>(new JsonEventSerializer(new[]
                    {
                        typeof(CustomerCreated).Assembly
                    }));

                    services.AddHttpClient<ICustomersApiClient, CustomersApiClient>("customersApiClient", (ctx, httpClient) =>
                        {
                            var config = ctx.GetRequiredService<IConfiguration>();
                            var endpoint = config["CustomersApi"];
                            httpClient.BaseAddress = new System.Uri(endpoint);
                        })
                        .AddPolicyHandler(HttpClientPolicies.GetRetryPolicy());

                    services.AddSingleton<INotificationsFactory, NotificationsFactory>();
                    services.AddSingleton<INotificationsService, FakeNotificationsService>();

                    services.AddSingleton(ctx =>
                    {
                        var kafkaConnStr = hostContext.Configuration.GetConnectionString("kafka");
                        var eventsTopicName = hostContext.Configuration["eventsTopicName"];
                        var groupName = hostContext.Configuration["eventsTopicGroupName"];
                        return new EventConsumerConfig(kafkaConnStr, eventsTopicName, groupName);
                    });

                    services.AddHostedService(ctx =>
                    {
                        var logger = ctx.GetRequiredService<ILogger<EventConsumer<Account, Guid>>>();
                        var eventsDeserializer = ctx.GetRequiredService<IEventSerializer>();
                        var consumerConfig = ctx.GetRequiredService<EventConsumerConfig>();
                        var notificationsFactory = ctx.GetRequiredService<INotificationsFactory>();
                        var notificationsService = ctx.GetRequiredService<INotificationsService>();
                        var consumer = new EventConsumer<Account, Guid>(eventsDeserializer, consumerConfig, logger);

                        return new AccountEventsWorker(notificationsFactory, notificationsService, consumer, logger);
                    });
                });
    }
}
