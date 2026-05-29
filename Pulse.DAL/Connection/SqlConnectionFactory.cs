using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Pulse.DAL.Connection
{
    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public SqlConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection CreateConnection()
        {
            var connectionString =
                _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
            "Connection string 'DefaultConnection' not found.");

            return new SqlConnection(connectionString);
        }
    }
}