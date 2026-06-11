using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.Members;

public interface IMemberCommands : ICommands
{
    Task CreateMemberAsync(CreateMemberInput input);
}
