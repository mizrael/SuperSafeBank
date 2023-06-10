using Dapper;
using Microsoft.Data.SqlClient;
using SuperSafeBank.Domain;
using System.ComponentModel;

namespace SuperSafeBank.Persistence.SQLServer.Tests.Integration
{
    [Category("Integration")]
    [Trait("Category", "Integration")]
    public class AggregateTableCreatorTests : IClassFixture<DbFixture>
    {
        private readonly DbFixture _fixture;

        public AggregateTableCreatorTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task EnsureTableAsync_should_create_table()
        {
            var provider = await _fixture.CreateDbConnectionStringProviderAsync();
            var sut = new AggregateTableCreator(provider);
            await sut.EnsureTableAsync<Customer, Guid>();

            var tableName = sut.GetTableName<Customer, Guid>();
            using var conn = new SqlConnection(provider.ConnectionString);
            await conn.OpenAsync();
            var res = await conn.QueryFirstOrDefaultAsync<int?>($"SELECT COUNT(1) FROM {tableName};").ConfigureAwait(false);
            res.Should().Be(0);
        }
    }
}
