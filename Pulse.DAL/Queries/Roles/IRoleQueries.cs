
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Queries.Roles;

public interface IRoleQueries : IQueries
{
    Task<IEnumerable<RoleRecord>> GetRolesAsync();
    Task<RoleRecord> GetRoleByNameAsync(string name);
}
