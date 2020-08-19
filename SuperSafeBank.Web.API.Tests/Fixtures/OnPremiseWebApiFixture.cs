#if OnPremise

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace SuperSafeBank.Web.API.Tests.Fixtures
{
    public class OnPremiseWebApiFixture<TStartup> : BaseWebApiFixture<TStartup> where TStartup : class
    {
        private string _queryDbName;
        private string _queryDbConnectionString;

        protected override void OnConfigureAppConfiguration(IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("queryDbName", $"bankAccounts_{Guid.NewGuid()}"),
                new KeyValuePair<string, string>("eventsTopicName", $"events_{Guid.NewGuid()}")
            });

            var cfg = configurationBuilder.Build();
            _queryDbName = cfg["queryDbName"];
            _queryDbConnectionString = cfg.GetConnectionString("mongo");
        }

        public void Dispose()
        {
            if (!string.IsNullOrWhiteSpace(_queryDbConnectionString))
            {
                var client = new MongoClient(_queryDbConnectionString);
                client.DropDatabase(_queryDbName);
            }
        }
    }
}

#endif
