using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using SuperSafeBank.Common;
using SuperSafeBank.Domain.DomainEvents;
using SuperSafeBank.Domain.Services;
using SuperSafeBank.Service.Core.Common;
using SuperSafeBank.Service.Core.Common.EventHandlers;
using SuperSafeBank.Service.Core.Persistence.Mongo.EventHandlers;
using SuperSafeBank.Worker.Core.Registries;

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
        if(string.IsNullOrWhiteSpace(connStr))
            throw new ArgumentNullException("loki connection string is not set");
        cfg.WriteTo.GrafanaLoki(connStr);
    })
    .ConfigureServices((hostContext, services) =>
    {       
        services.Scan(scan =>
        {
            scan.FromAssembliesOf(typeof(AccountEventsHandler))                
                .RegisterHandlers(typeof(INotificationHandler<>));
        }).Decorate(typeof(INotificationHandler<>), typeof(RetryDecorator<>))
            .AddTransient<ICurrencyConverter, FakeCurrencyConverter>()            
            .AddScoped<IMediator, Mediator>()
            .AddSingleton<IEventSerializer>(new JsonEventSerializer(new[]
            {
                typeof(CustomerEvents.CustomerCreated).Assembly
            }))
            .RegisterInfrastructure(hostContext.Configuration)
            .RegisterWorker();
    })
    .Build()
    .RunAsync();

