using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace SuperSafeBank.Service.Core.Persistence.Mongo
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// https://stackoverflow.com/a/46470759/3279163
        /// </summary>
        internal class CustomSerializationProvider : IBsonSerializationProvider
        {
            private static readonly IBsonSerializer<decimal> decimalSerializer = new DecimalSerializer(BsonType.Decimal128);
            private static readonly IBsonSerializer nullableSerializer = new NullableSerializer<decimal>(decimalSerializer);
            private static readonly IBsonSerializer guidSerializer = new GuidSerializer(GuidRepresentation.Standard);
            
            public IBsonSerializer GetSerializer(Type type)
            {
                if (type == typeof(decimal)) return decimalSerializer;
                if (type == typeof(decimal?)) return nullableSerializer;
                if (type == typeof(Guid)) return guidSerializer;

                return null; // falls back to Mongo defaults
            }
        }

        public static IServiceCollection AddMongoDb(this IServiceCollection services, MongoConfig configuration)
        {
            //https://stackoverflow.com/questions/63443445/trouble-with-mongodb-c-sharp-driver-when-performing-queries-using-guidrepresenta
            BsonDefaults.GuidRepresentationMode = GuidRepresentationMode.V3;

            BsonSerializer.RegisterSerializationProvider(new CustomSerializationProvider());
            
            return services.AddSingleton(ctx => new MongoClient(connectionString: configuration.ConnectionString))
                .AddSingleton(ctx =>
                {
                    
                    var client = ctx.GetRequiredService<MongoClient>();
                    var database = client.GetDatabase(configuration.QueryDbName);
                    return database;
                }).AddSingleton<IQueryDbContext, QueryDbContext>();
        }
    }

    public record MongoConfig(string ConnectionString, string QueryDbName);
}
