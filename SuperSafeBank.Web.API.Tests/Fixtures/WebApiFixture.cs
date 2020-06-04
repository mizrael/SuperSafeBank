using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace SuperSafeBank.Web.API.Tests.Fixtures
{
    public class WebApiFixture<TStartup> : IDisposable 
        where TStartup : class
    {
        private string _queryDbName;
        private string _queryDbConnectionString;

        public WebApiFixture()
        {
            var builder = new WebHostBuilder()
                .UseEnvironment("Development")
                .ConfigureAppConfiguration((ctx, configurationBuilder) =>
                {
                    configurationBuilder.AddJsonFile("appsettings.json", false);

                    var aspEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                    if (!string.IsNullOrWhiteSpace(aspEnv))
                        configurationBuilder.AddJsonFile($"appsettings.{aspEnv}.json", true);

                    configurationBuilder.AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string>("queryDbName", $"bankAccounts_{Guid.NewGuid()}"),
                        new KeyValuePair<string, string>("eventsTopicName", $"events_{Guid.NewGuid()}")
                    });

                    var cfg = configurationBuilder.Build();
                    _queryDbName = cfg["queryDbName"];
                    _queryDbConnectionString = cfg.GetConnectionString("mongo");
                })
                .UseStartup<TStartup>();
            
            var server = new TestServer(builder);
            
            this.HttpClient = server.CreateClient();
        }

        public void Dispose()
        {
            if (!string.IsNullOrWhiteSpace(_queryDbConnectionString))
            {
                var client = new MongoClient(_queryDbConnectionString);
                client.DropDatabase(_queryDbName);
            }
            
            if (null != this.HttpClient)
            {
                this.HttpClient.Dispose();
                this.HttpClient = null;
            }
        }

        public HttpClient HttpClient { get; private set; }
    }
}