using Microsoft.Extensions.Configuration;

namespace SuperSafeBank.Web.API.Tests.Fixtures
{
    internal interface IConfigurationStrategy
    {
        void OnConfigureAppConfiguration(IConfigurationBuilder configurationBuilder);

        IQueryModelsSeeder CreateSeeder();
    }
}