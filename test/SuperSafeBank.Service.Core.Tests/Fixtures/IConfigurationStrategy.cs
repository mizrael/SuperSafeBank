using Microsoft.Extensions.Configuration;

namespace SuperSafeBank.Service.Core.Tests.Fixtures
{
    internal interface IConfigurationStrategy
    {
        void OnConfigureAppConfiguration(IConfigurationBuilder configurationBuilder);

        IQueryModelsSeeder CreateSeeder();
    }
}