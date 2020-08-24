using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Core;
using SuperSafeBank.Core.Models;
using SuperSafeBank.Domain;

namespace SuperSafeBank.Persistence.EventStore
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddEventStore(this IServiceCollection services, string connectionString)
        {
            return services.AddSingleton<IEventStoreConnectionWrapper>(ctx =>
                {
                    var logger = ctx.GetRequiredService<ILogger<EventStoreConnectionWrapper>>();
                    return new EventStoreConnectionWrapper(new Uri(connectionString), logger);
                }).AddEventsRepository<Customer, Guid>()
                .AddEventsRepository<Account, Guid>();
        }

        private static IServiceCollection AddEventsRepository<TA, TK>(this IServiceCollection services)
            where TA : class, IAggregateRoot<TK>
        {
            return services.AddSingleton<IEventsRepository<TA, TK>>(ctx =>
            {
                var connectionWrapper = ctx.GetRequiredService<IEventStoreConnectionWrapper>();
                var eventDeserializer = ctx.GetRequiredService<IEventSerializer>();
                return new EventsRepository<TA, TK>(connectionWrapper, eventDeserializer);
            });
        }
    }
}