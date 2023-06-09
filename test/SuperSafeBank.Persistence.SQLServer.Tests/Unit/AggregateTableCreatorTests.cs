using SuperSafeBank.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSafeBank.Persistence.SQLServer.Tests.Unit
{
    public class AggregateTableCreatorTests
    {
        [Fact]
        public void GetTableName_should_return_valid_table_name()
        {
            var dbConn = Substitute.For<IDbConnection>();
            var sut = new AggregateTableCreator(dbConn, "testDbo");
            var table = sut.GetTableName<Customer, Guid>();
            table.Should().Be("testDbo.customer");
        }
    }
}
