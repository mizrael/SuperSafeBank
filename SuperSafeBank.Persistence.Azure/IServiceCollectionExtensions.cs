using Azure.Data.Tables;
using Microsoft.Extensions.DependencyInjection;
using SuperSafeBank.Common;
using SuperSafeBank.Common.Models;
using SuperSafeBank.Domain;
using System;

namespace SuperSafeBank.Persistence.Azure
{
    public record EventsRepositoryConfig(Uri ConnectionString, string TablePrefix = "");

    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddAzurePersistence(this IServiceCollection services, EventsRepositoryConfig config)
        {
            return services
                .AddEventsRepository<Customer, Guid>(config)
                .AddEventsRepository<Account, Guid>(config);
        }

        private static IServiceCollection AddEventsRepository<TA, TK>(this IServiceCollection services, EventsRepositoryConfig config)
            where TA : class, IAggregateRoot<TK>
        {
            var aggregateType = typeof(TA);
            var tableName = $"{config.TablePrefix}{aggregateType.Name}Events";
            
            return services.AddSingleton<IEventsRepository<TA, TK>>(ctx =>
            {
                var client = new TableClient(config.ConnectionString);
                var eventDeserializer = ctx.GetRequiredService<IEventSerializer>();
                return new EventsRepository<TA, TK>(client, eventDeserializer);
            });
        }
    }
}