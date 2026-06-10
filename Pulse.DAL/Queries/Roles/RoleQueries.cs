
using Dapper;
using Pulse.DAL.Connection;

namespace Pulse.DAL.Queries.Roles;

public class RoleQueries : IRoleQueries
{
    private readonly IDbConnectionFactory _connectionFactory;
    public RoleQueries(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<RoleRecord>> GetRolesAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var roles = await connection.QueryAsync<RoleRecord>("SELECT * FROM Roles");
        return roles.ToList();
    }

    public async Task<RoleRecord> GetRoleByNameAsync(string name)
    {
        using var connection = _connectionFactory.CreateConnection();
        var role = await connection.QuerySingleOrDefaultAsync<RoleRecord>("SELECT * FROM Roles WHERE Name = @Name",
            new { Name = name });
        return role;
    }
}
