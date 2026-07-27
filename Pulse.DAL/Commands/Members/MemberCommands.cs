using Dapper;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.Members;

public class MemberCommands : IMemberCommands
{
    private readonly IDbSessionAccessor _sessionAccessor;

    public MemberCommands(IDbSessionAccessor sessionAccessor)
    {
        _sessionAccessor = sessionAccessor;
    }

    /// <inheritdoc/>
    public async Task CreateMemberAsync(CreateMemberInput input, CancellationToken ct)
    {
        IDbSession session = _sessionAccessor.Session
            ?? throw new InvalidOperationException("No active unit of work.");

        await session.Connection.ExecuteAsync(
            new CommandDefinition(
                "INSERT INTO Members (UserId, OrganizationId, RoleId, JoinedAt, UpdatedAt) " +
                "VALUES (@UserId, @OrganizationId, @RoleId, @Now, @Now)",
                new { input.UserId, input.OrganizationId, input.RoleId, Now = DateTimeOffset.UtcNow },
                transaction: session.Transaction,
                cancellationToken: ct));
    }
}
