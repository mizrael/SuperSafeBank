using SuperSafeBank.Common;
using SuperSafeBank.Persistence.Tests.Models;
using System.ComponentModel;

namespace SuperSafeBank.Persistence.SQLServer.Tests.Integration
{
    //TODO: should centralize integration tests somehow (base class?)
    [Category("Integration")]
    [Trait("Category", "Integration")]
    public class SQLAggregateRepositoryTests : 
        AggregateRepositoryBaseTests<SQLAggregateRepository<DummyAggregate, Guid>>, 
        IClassFixture<DbFixture>
    {
        private readonly DbFixture _fixture;

        public SQLAggregateRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        protected override async ValueTask<SQLAggregateRepository<DummyAggregate, Guid>> CreateSut()
        {
            var conn = await _fixture.CreateDbConnectionStringProviderAsync();
            var tableCreator = new AggregateTableCreator(conn);
            var serializer = new JsonEventSerializer(new[]
            {
                typeof(DummyAggregate).Assembly
            });
            var sut = new SQLAggregateRepository<DummyAggregate, Guid>(conn, tableCreator, serializer);
            return sut;
        }
    }
}
