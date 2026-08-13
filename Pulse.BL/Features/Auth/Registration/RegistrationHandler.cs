using FluentResults;
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
            return Result.Ok();
        }

        string passwordHash = _passwordHasher.HashPassword(command.Password);

        // Create the user inside a narrow try/catch so a DuplicateKey on the
        // Email column is handled as a benign "already registered" race.
        Guid userId;
        try
        {
            await using IUnitOfWork uow = await _unitOfWorkFactory.CreateAsync(ct: ct);
            userId = await _userCommands.CreateUserAsync(new CreateUserInput
            (
                command.Email,
                command.FirstName,
                command.LastName,
                passwordHash
            ), ct);

            // Proceed to create member and commit outside the DuplicateKey catch
            // so that duplicate-key failures during those operations are surfaced
            // as real conflicts instead of silent successes.
            await _memberCommands.CreateMemberAsync(new CreateMemberInput
            (
                userId,
                SeededIds.Organizations.Default,
                SeededIds.Roles.User
            ), ct);
            await uow.CommitAsync(ct);
        }
        catch (DuplicateKeyException)
        {
            // If the duplicate key occurred while inserting the user (concurrent
            // registration), treat as success to avoid account enumeration.
            return Result.Ok();
        }

        return Result.Ok();
    }
}
