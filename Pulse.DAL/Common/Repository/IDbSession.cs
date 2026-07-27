using System.Data;

namespace Pulse.DAL.Common.Repository;

public interface IDbSession
{
    /// <summary>
    /// The active database connection for this unit of work.
    /// </summary>
    IDbConnection Connection { get; }

    /// <summary>
    /// The active database transaction for this unit of work.
    /// </summary>
    IDbTransaction Transaction { get; }
}
