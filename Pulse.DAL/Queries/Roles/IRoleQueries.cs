
namespace Pulse.DAL.Queries.Roles;

public interface IRoleQueries
{
    Task<IEnumerable<RoleRecord>> GetRolesAsync();
    Task<RoleRecord> GetRoleByNameAsync(string name);
}
