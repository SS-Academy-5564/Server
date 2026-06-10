namespace Pulse.DAL.Commands.Members;

public interface IMemberCommands
{
    Task CreateMemberAsync(CreateMemberInput input);
}
