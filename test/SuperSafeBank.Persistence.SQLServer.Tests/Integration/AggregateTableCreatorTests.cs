using Dapper;
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
            using var conn = await _fixture.CreateDbConnectionAsync();
            var sut = new AggregateTableCreator(conn);
            await sut.EnsureTableAsync<Customer, Guid>();

            var tableName = sut.GetTableName<Customer, Guid>();
            var res = await conn.QueryFirstOrDefaultAsync<int?>($"SELECT COUNT(1) FROM {tableName};").ConfigureAwait(false);
            res.Should().Be(0);
        }
    }
}
