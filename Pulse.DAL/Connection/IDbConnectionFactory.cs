using System.Data;

namespace Pulse.DAL.Connection;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
