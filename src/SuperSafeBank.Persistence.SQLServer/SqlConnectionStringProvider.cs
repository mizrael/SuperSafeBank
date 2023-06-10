namespace SuperSafeBank.Persistence.SQLServer
{
    public class SqlConnectionStringProvider
    {
        public string ConnectionString { get; }

        public SqlConnectionStringProvider(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))            
                throw new ArgumentException($"'{nameof(connectionString)}' cannot be null or whitespace.", nameof(connectionString));

            ConnectionString = connectionString;
        }
    }
}