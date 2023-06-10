using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SuperSafeBank.Common;
using System;

namespace SuperSafeBank.Persistence.EventStore
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddEventStorePersistence(this IServiceCollection services, string connectionString)
        {
            return services.AddSingleton<IEventStoreConnectionWrapper>(ctx =>
                {
                    var logger = ctx.GetRequiredService<ILogger<EventStoreConnectionWrapper>>();
                    return new EventStoreConnectionWrapper(new Uri(connectionString), logger);
                }).AddSingleton(typeof(IAggregateRepository<,>), typeof(EventStoreAggregateRepository<,>));
        }
    }
}