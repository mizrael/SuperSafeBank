using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuperSafeBank.Persistence.Azure;
using SuperSafeBank.Service.Core.Azure.Common.Persistence;
using SuperSafeBank.Service.Core.Common;
using SuperSafeBank.Worker.Core.Azure.EventHandlers;

var builder = new HostBuilder();
await builder.ConfigureFunctionsWorkerDefaults()
        .ConfigureServices((ctx, services) =>
        {
            var eventsRepositoryConfig = new EventsRepositoryConfig(ctx.Configuration["EventsStorage"], ctx.Configuration["EventTablesPrefix"]);

            services.AddScoped<ServiceFactory>(ctx => ctx.GetRequiredService)
                .AddScoped<IMediator, Mediator>()
                .Scan(scan =>
                {
                    scan.FromAssembliesOf(typeof(CustomerDetailsHandler))                       
                        .RegisterHandlers(typeof(INotificationHandler<>));
                })
                .AddSingleton<IViewsContext>(provider =>
                {
                    var connStr = ctx.Configuration["QueryModelsStorage"];
                    var tablesPrefix = ctx.Configuration["QueryModelsTablePrefix"];
                    return new ViewsContext(connStr, tablesPrefix);
                }).AddAzurePersistence(eventsRepositoryConfig);
        })
        .Build()
        .RunAsync();