using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;

namespace SuperSafeBank.Web.API.Tests.Fixtures
{
    public class WebApiFixture<TStartup> : IDisposable 
        where TStartup : class
    {
        public WebApiFixture()
        {
            var builder = new WebHostBuilder()
                .UseEnvironment("Development")
                .ConfigureAppConfiguration((ctx, cfg) =>
                {
                    cfg.AddJsonFile("appsettings.json", false);

                    var aspEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                    if (!string.IsNullOrWhiteSpace(aspEnv))
                        cfg.AddJsonFile($"appsettings.{aspEnv}.json", true);
                })
                .UseStartup<TStartup>();

            var server = new TestServer(builder);

            this.HttpClient = server.CreateClient();
        }

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