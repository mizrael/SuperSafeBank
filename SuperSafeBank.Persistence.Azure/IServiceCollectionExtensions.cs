using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SuperSafeBank.Core;
using SuperSafeBank.Core.Models;
using SuperSafeBank.Domain;
using System;

namespace SuperSafeBank.Persistence.Azure
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddAzurePersistence(this IServiceCollection services, IConfiguration config)
        {
            return services.AddSingleton(ctx =>
                {
                    var options = new CosmosClientOptions()
                    {
                        Serializer = new CustomJsonSerializer()
                    };
                    var connectionString = config.GetConnectionString("cosmos");
                    return new CosmosClient(connectionString, options);
                })
                .AddEventsRepository<Customer, Guid>(config)
                .AddEventsRepository<Account, Guid>(config);
        }

        private static IServiceCollection AddEventsRepository<TA, TK>(this IServiceCollection services, IConfiguration config)
            where TA : class, IAggregateRoot<TK>
        {
            return services.AddSingleton<IDbContainerProvider>(ctx =>
            {
                var cosmos = ctx.GetRequiredService<CosmosClient>();
                var dbName = config["dbName"];
                var db = cosmos.GetDatabase(dbName);
                return new DbContainerProvider(db);
            }).AddSingleton<IEventsRepository<TA, TK>>(ctx =>
            {
                var containerProvider = ctx.GetRequiredService<IDbContainerProvider>();
                var eventDeserializer = ctx.GetRequiredService<IEventSerializer>();
                return new EventsRepository<TA, TK>(containerProvider, eventDeserializer);
            });
        }
    }
}