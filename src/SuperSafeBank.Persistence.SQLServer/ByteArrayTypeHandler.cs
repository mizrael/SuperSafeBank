using Dapper;
using System.Data;
using System.Text;

namespace SuperSafeBank.Persistence.SQLServer
{
    public class ByteArrayTypeHandler : SqlMapper.TypeHandler<byte[]>
    {
        public override void SetValue(IDbDataParameter parameter, byte[] value) => 
            parameter.Value = Encoding.UTF8.GetString(value, 0, value.Length);

        public override byte[] Parse(object value) => 
            Encoding.UTF8.GetBytes((string)value);
    }
}