using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace SuperSafeBank.Web.API.Tests.Fixtures
{
    public abstract class BaseWebApiFixture<TStartup> : IDisposable
        where TStartup : class
    {
        protected BaseWebApiFixture()
        {
            var builder = new WebHostBuilder()
                .UseEnvironment("Development")
                .ConfigureAppConfiguration((ctx, configurationBuilder) =>
                {
                    configurationBuilder.AddJsonFile("appsettings.json", false);

                    var aspEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                    if (!string.IsNullOrWhiteSpace(aspEnv))
                        configurationBuilder.AddJsonFile($"appsettings.{aspEnv}.json", true);

                   
                })
                .UseSerilog()
                .UseStartup<TStartup>();

            var server = new TestServer(builder);
            this.HttpClient = server.CreateClient();
        }

        protected abstract void OnConfigureAppConfiguration(IConfigurationBuilder configurationBuilder);

        public void Dispose()
        {
            if (null != this.HttpClient)
            {
                this.HttpClient.Dispose();
                this.HttpClient = null;
            }
        }

        public HttpClient HttpClient { get; private set; }
    }
}