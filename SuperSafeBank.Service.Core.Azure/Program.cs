using Azure.Data.Tables;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuperSafeBank.Domain.Commands;
using SuperSafeBank.Domain.Services;
using SuperSafeBank.Persistence.Azure;
using SuperSafeBank.Service.Core.Azure.Common.Persistence;
using SuperSafeBank.Service.Core.Azure.QueryHandlers;
using SuperSafeBank.Service.Core.Azure.Services;
using SuperSafeBank.Service.Core.Common;
using SuperSafeBank.Transport.Azure;
using System.Reflection;

var builder = new HostBuilder();

await builder.ConfigureFunctionsWorkerDefaults()
        .ConfigureHostConfiguration(builder =>
        {
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
        })
        .ConfigureServices((ctx, services) =>
        {
            var eventsRepositoryConfig = new EventsRepositoryConfig(ctx.Configuration["EventsStorage"], ctx.Configuration["EventTablesPrefix"]);
            var eventProducerConfig = new EventProducerConfig(ctx.Configuration["EventsBus"], ctx.Configuration["TopicName"]);
            
            services.AddScoped<ServiceFactory>(ctx => ctx.GetRequiredService)
                .AddScoped<IMediator, Mediator>()
                .Scan(scan =>
                {
                    scan.FromAssembliesOf(typeof(CustomerByIdHandler), typeof(CreateCustomerHandler))
                        .RegisterHandlers(typeof(IRequestHandler<>))
                        .RegisterHandlers(typeof(IRequestHandler<,>))
                        .RegisterHandlers(typeof(INotificationHandler<>));
                })
            .AddTransient<ICustomerEmailsService>(provider =>
            {
                var connStr = ctx.Configuration["EventsStorage"]; 
                var client = new TableClient(connStr, "CustomerEmails");
                client.CreateIfNotExists();
                return new CustomerEmailsService(client);
            })
            .AddSingleton<IViewsContext>(provider =>
            {
                var connStr = ctx.Configuration["QueryModelsStorage"];
                var tablesPrefix = ctx.Configuration["QueryModelsTablePrefix"];
                return new ViewsContext(connStr, tablesPrefix);
            }).AddAzurePersistence(eventsRepositoryConfig)
              .AddAzureTransport(eventProducerConfig);
        })
        .Build()
        .RunAsync();