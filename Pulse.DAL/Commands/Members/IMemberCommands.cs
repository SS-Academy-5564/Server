using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.Members;

public interface IMemberCommands : ICommands
{
    /// <summary>
    /// Inserts a new member record linking a user to an organization with a given role.
    /// </summary>
    /// <param name="input">The data required to create the member.</param>
    /// <param name="uow">The unit of work providing the connection and transaction.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task CreateMemberAsync(CreateMemberInput input, IUnitOfWork uow, CancellationToken ct);
}
