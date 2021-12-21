using Microsoft.Extensions.Configuration;
using SuperSafeBank.Service.Core.Azure.Common.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperSafeBank.Service.Core.Azure.Tests
{
    public class StorageTableFixutre : Xunit.IAsyncLifetime
    {        
        private readonly Queue<IViewsContext> _dbContexts = new();
        
        private readonly string _tablePrefix;
        private readonly string _connStr;

        public StorageTableFixutre()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddUserSecrets<StorageTableFixutre>(optional: true)
                .AddEnvironmentVariables()
                .Build();
            
            _connStr = configuration.GetConnectionString("storageTable");
            if(string.IsNullOrWhiteSpace(_connStr))
                throw new ArgumentException("invalid storage account connection string");

            _tablePrefix = configuration["tablePrefix"];
        }

        public IViewsContext CreateTableClient() 
        {
           var ctx = new ViewsContext(_connStr, _tablePrefix);

            _dbContexts.Enqueue(ctx);

            return ctx;
        }

        public async Task DisposeAsync()
        {
            while (_dbContexts.Any())
            {
                var ctx = _dbContexts.Dequeue();
                await ctx.CustomersDetails.DeleteAsync();
                await ctx.CustomersArchive.DeleteAsync();
                await ctx.Accounts.DeleteAsync();
            }
        }

        public Task InitializeAsync() => Task.CompletedTask;
    }
}