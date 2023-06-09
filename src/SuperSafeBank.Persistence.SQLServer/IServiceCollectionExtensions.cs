using Dapper;
using Microsoft.Extensions.DependencyInjection;
using SuperSafeBank.Common;

namespace SuperSafeBank.Persistence.SQLServer
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddSQLServer(this IServiceCollection services, string connectionString)
        { 
            SqlMapper.AddTypeHandler(new ByteArrayTypeHandler());

            return services.AddSingleton(new SqlConnectionStringProvider(connectionString))
                           .AddSingleton<IAggregateTableCreator, AggregateTableCreator>()
                           .AddSingleton(typeof(IAggregateRepository<,>), typeof(SQLAggregateRepository<,>));
        }
    }
}