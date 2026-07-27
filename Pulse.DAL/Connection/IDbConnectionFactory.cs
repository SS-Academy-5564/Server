using System.Data.Common;

namespace Pulse.DAL.Connection;

public interface IDbConnectionFactory
{
    DbConnection CreateConnection();
}
