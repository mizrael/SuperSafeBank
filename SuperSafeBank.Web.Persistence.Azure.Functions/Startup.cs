using System;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using SuperSafeBank.Core;
using SuperSafeBank.Domain.Events;
using SuperSafeBank.Persistence.Azure;
using SuperSafeBank.Web.Persistence.Azure.Functions;
using SuperSafeBank.Web.Persistence.Azure.Functions.EventHandlers;

[assembly: FunctionsStartup(typeof(Startup))]
namespace SuperSafeBank.Web.Persistence.Azure.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IEventSerializer>(new JsonEventSerializer(new[]
            {
                typeof(CustomerCreated).Assembly
            }));

            builder.Services.AddSingleton<CosmosClient>(ctx =>
            {
                var connectionString = Environment.GetEnvironmentVariable("cosmos");
                return new CosmosClient(connectionString);
            });
            builder.Services.AddSingleton<IDbContainerProvider>(ctx =>
            {
                var cosmos = ctx.GetRequiredService<CosmosClient>();
                var dbName = Environment.GetEnvironmentVariable("cosmosDbName");
                var db = cosmos.GetDatabase(dbName);
                return new DbContainerProvider(db);
            });

            builder.Services.AddMediatR(typeof(CustomersArchiveHandler));
        }
    }
}