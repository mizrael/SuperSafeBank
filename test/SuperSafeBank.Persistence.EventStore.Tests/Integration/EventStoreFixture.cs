using Microsoft.Extensions.Configuration;

namespace SuperSafeBank.Persistence.EventStore.Tests.Integration
{
    public class EventStoreFixture
    {
        public string ConnectionString { get; }

        public EventStoreFixture()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            ConnectionString = configuration.GetConnectionString("eventStore");
        }
    }
}