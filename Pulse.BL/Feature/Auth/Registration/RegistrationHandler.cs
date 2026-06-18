using FluentResults;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Security;
using Pulse.DAL.Commands.Members;
using Pulse.DAL.Commands.Users;
using Pulse.DAL.Common.Constants;
using Pulse.DAL.Common.Repository;
using Pulse.DAL.Exceptions;
using Pulse.DAL.Queries.Users;

namespace Pulse.BL.Feature.Auth.Registration;

public class RegistrationHandler : IRegistrationHandler
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IUserCommands _userCommands;
    private readonly IUserQueries _userQueries;
    private readonly IPasswordHasher _passwordHasher;

    private readonly IMemberCommands _memberCommands;

    public RegistrationHandler(
        IUnitOfWorkFactory unitOfWorkFactory,
        IUserCommands userCommands,
        IUserQueries userQueries,
        IPasswordHasher passwordHasher,
        IMemberCommands memberCommands)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _userCommands = userCommands;
        _userQueries = userQueries;
        _passwordHasher = passwordHasher;
        _memberCommands = memberCommands;
    }
    /// <inheritdoc/>
    public async Task<Result> RegisterAsync(RegistrationCommand command, CancellationToken ct)
    {
        bool userExists = await _userQueries.EmailExistsAsync(command.Email, ct);

        if (userExists)
        {
            return Result.Fail(new ConflictError("A user with this Email already exists."));
        }

        string passwordHash = _passwordHasher.HashPassword(command.Password);

        try
        {
            await _unitOfWorkFactory.ExecuteAsync(async uow =>
            {
                Guid userId = await _userCommands.CreateUserAsync(new CreateUserInput
                {
                    Email = command.Email,
                    FirstName = command.FirstName,
                    LastName = command.LastName,
                    PasswordHash = passwordHash
                }, uow.Transaction, ct);

                await _memberCommands.CreateMemberAsync(new CreateMemberInput
                {
                    UserId = userId,
                    OrganizationId = SeededIds.Organizations.Default,
                    RoleId = SeededIds.Roles.User
                }, uow.Transaction, ct);
            }, ct);
        }
        catch (DuplicateKeyException ex)
        {
            return Result.Fail(new ConflictError($"A user with this {ex.FieldName} already exists."));
        }

        return Result.Ok();
    }
}
