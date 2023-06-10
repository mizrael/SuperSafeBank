using SuperSafeBank.Domain;

namespace SuperSafeBank.Persistence.SQLServer.Tests.Unit
{
    public class AggregateTableCreatorTests
    {
        [Fact]
        public void GetTableName_should_return_valid_table_name()
        {
            var dbConn = new SqlConnectionStringProvider("lorem");
            var sut = new AggregateTableCreator(dbConn, "testDbo");
            var table = sut.GetTableName<Customer, Guid>();
            table.Should().Be("testdbo.customer");
        }
    }
}
