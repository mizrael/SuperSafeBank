using EvenireDB.Client;
using Microsoft.Extensions.DependencyInjection;
using SuperSafeBank.Common;

namespace SuperSafeBank.Persistence.EvenireDB;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddEvenireDBPersistence(this IServiceCollection services, EvenireClientConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        return services.AddEvenireDB(config)
                       .AddSingleton(typeof(IAggregateRepository<,>), typeof(AggregateRepository<,>));
    }
}
