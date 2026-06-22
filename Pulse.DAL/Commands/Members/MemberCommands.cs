
using System.Data;
using Dapper;
using Pulse.DAL.Connection;

namespace Pulse.DAL.Commands.Members;

public class MemberCommands : IMemberCommands
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MemberCommands(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task CreateMemberAsync(CreateMemberInput input, CancellationToken ct)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            new CommandDefinition(
                "INSERT INTO Members (UserId, OrganizationId, RoleId, JoinedAt, UpdatedAt) " +
                "VALUES (@UserId, @OrganizationId, @RoleId, @Now, @Now)",
                new { input.UserId, input.OrganizationId, input.RoleId, Now = DateTimeOffset.UtcNow },
                cancellationToken: ct));
    }
}
