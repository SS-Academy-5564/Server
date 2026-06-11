using Pulse.BL.Common.Security;
using Pulse.DAL.Commands.Members;
using Pulse.DAL.Commands.Users;
using Pulse.DAL.Common.Constants;
using Pulse.DAL.Queries.Users;

namespace Pulse.BL.Features.Auth.Registration;

public class RegistrationHandler : IRegistrationHandler
{
    private readonly IUserCommands _userCommands;
    private readonly IUserQueries _userQueries;
    private readonly IPasswordHasher _passwordHasher;

    // remove later
    private readonly IMemberCommands _memberCommands;


    public RegistrationHandler(
        IUserCommands userCommands,
        IUserQueries userQueries,
        IPasswordHasher passwordHasher,
        IMemberCommands memberCommands)
    {
        _userCommands = userCommands;
        _userQueries = userQueries;
        _passwordHasher = passwordHasher;
        _memberCommands = memberCommands;
    }
    public async Task Register(RegistrationCommand command, CancellationToken ct)
    {
        var userExists = await _userQueries.EmailExistsAsync(command.Email, ct);

        if (userExists)
        {
            //return fail here
            return;
        }

        var passwordHash = _passwordHasher.HashPassword(command.Password);

        var userId = await _userCommands.CreateAsync(new CreateUserInput
        {
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            PasswordHash = passwordHash
        }, ct);

        if (userId == Guid.Empty)
        {
            // return failure
            return;
        }

        await _memberCommands.CreateMemberAsync(new CreateMemberInput
        {
            UserId = userId,
            OrganizationId = SeededIds.Organizations.Default,
            RoleId = SeededIds.Roles.User
        }, ct);

        // return success
    }
}
