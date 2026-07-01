using FluentResults;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security.Passwords;
using Pulse.DAL.Commands.Members;
using Pulse.DAL.Commands.Users;
using Pulse.DAL.Common.Constants;
using Pulse.DAL.Common.Repository;
using Pulse.DAL.Exceptions;
using Pulse.DAL.Queries.Users;

namespace Pulse.BL.Features.Auth.Registration;

public class RegistrationHandler : IAsyncHandler<RegistrationCommand, Result>
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

    /// <summary>
    /// Registers a new user with the provided registration details.
    /// </summary>
    /// <param name="request">The command containing the user's registration data.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A <see cref="Result"/> indicating success or a failure with error details.</returns>
    public async Task<Result> HandleAsync(RegistrationCommand command, CancellationToken ct)
    {
        bool userExists = await _userQueries.EmailExistsAsync(command.Email, ct);

        if (userExists)
        {
            return Result.Fail(new ConflictError("A user with this Email already exists."));
        }

        string passwordHash = _passwordHasher.HashPassword(command.Password);

        try
        {
            await using IUnitOfWork uow = await _unitOfWorkFactory.CreateAsync(ct);
            Guid userId = await _userCommands.CreateUserAsync(new CreateUserInput
            (
                command.Email,
                command.FirstName,
                command.LastName,
                passwordHash
            ), uow, ct);
            await _memberCommands.CreateMemberAsync(new CreateMemberInput
            (
                userId,
                SeededIds.Organizations.Default,
                SeededIds.Roles.User
            ), uow, ct);
            await uow.CommitAsync(ct);
        }
        catch (DuplicateKeyException ex)
        {
            return Result.Fail(new ConflictError($"A user with this {ex.FieldName} already exists."));
        }

        return Result.Ok();
    }
}
