using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security;
using Pulse.BL.Common.Security.Passwords;
using Pulse.BL.Features.Auth.EmailVerification;
using Pulse.BL.Features.Email;
using Pulse.DAL.Commands.EmailVerificationTokens;
using Pulse.DAL.Commands.Members;
using Pulse.DAL.Commands.Users;
using Pulse.DAL.Common.Constants;
using Pulse.DAL.Common.Repository;
using Pulse.DAL.Exceptions;
using Pulse.DAL.Queries.Users;

namespace Pulse.BL.Features.Auth.Registration;

/// <summary>
/// Registers users and issues their initial email verification message.
/// </summary>
public sealed class RegistrationHandler : IAsyncHandler<RegistrationCommand, Result>
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IUserCommands _userCommands;
    private readonly IUserQueries _userQueries;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMemberCommands _memberCommands;
    private readonly IEmailVerificationTokenCommands _verificationTokenCommands;
    private readonly IEmailVerificationTokenService _verificationTokenService;
    private readonly IEmailService _emailService;
    private readonly TimeProvider _timeProvider;
    private readonly EmailVerificationOptions _verificationOptions;
    private readonly ILogger<RegistrationHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegistrationHandler"/> class.
    /// </summary>
    /// <param name="unitOfWorkFactory">The factory used to create the registration transaction.</param>
    /// <param name="userCommands">The user persistence operations.</param>
    /// <param name="userQueries">The user lookup operations.</param>
    /// <param name="passwordHasher">The service used to hash the user's password.</param>
    /// <param name="memberCommands">The membership persistence operations.</param>
    /// <param name="verificationTokenCommands">The email verification token persistence operations.</param>
    /// <param name="verificationTokenService">The service used to generate and hash secure tokens.</param>
    /// <param name="emailService">The service used to deliver the verification email.</param>
    /// <param name="timeProvider">The source of the current UTC time.</param>
    /// <param name="verificationOptions">The configured token lifetime and verification page URL.</param>
    /// <param name="logger">The logger used to record delivery failures without sensitive tokens.</param>
    public RegistrationHandler(
        IUnitOfWorkFactory unitOfWorkFactory,
        IUserCommands userCommands,
        IUserQueries userQueries,
        IPasswordHasher passwordHasher,
        IMemberCommands memberCommands,
        IEmailVerificationTokenCommands verificationTokenCommands,
        IEmailVerificationTokenService verificationTokenService,
        IEmailService emailService,
        TimeProvider timeProvider,
        IOptions<EmailVerificationOptions> verificationOptions,
        ILogger<RegistrationHandler> logger)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _userCommands = userCommands;
        _userQueries = userQueries;
        _passwordHasher = passwordHasher;
        _memberCommands = memberCommands;
        _verificationTokenCommands = verificationTokenCommands;
        _verificationTokenService = verificationTokenService;
        _emailService = emailService;
        _timeProvider = timeProvider;
        _verificationOptions = verificationOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user with the provided registration details.
    /// </summary>
    /// <param name="command">The command containing the user's registration data.</param>
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
        string verificationToken = _verificationTokenService.GenerateToken();
        string verificationTokenHash = _verificationTokenService.ComputeHash(verificationToken);
        DateTimeOffset now = _timeProvider.GetUtcNow();
        DateTimeOffset expiresAt = now.AddHours(_verificationOptions.TokenLifetimeHours);
        string verificationUrl = EmailVerificationEmailBuilder.BuildVerificationUrl(
            _verificationOptions.VerificationPageUrl,
            verificationToken);

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

            await _memberCommands.CreateMemberAsync(new CreateMemberInput
            (
                userId,
                SeededIds.Organizations.Default,
                SeededIds.Roles.User
            ), ct);

            await _verificationTokenCommands.CreateAsync(
                new CreateEmailVerificationTokenInput(
                    userId,
                    verificationTokenHash,
                    expiresAt,
                    now),
                ct);

            Result emailResult = await _emailService.SendEmailAsync(
                new SendEmailDto(
                    To: [command.Email],
                    Subject: EmailVerificationEmailBuilder.BuildSubject(),
                    HtmlBody: EmailVerificationEmailBuilder.BuildHtmlBody(
                        verificationUrl,
                        _verificationOptions.TokenLifetimeHours),
                    PlainTextBody: EmailVerificationEmailBuilder.BuildPlainTextBody(
                        verificationUrl,
                        _verificationOptions.TokenLifetimeHours),
                    ReplyTo: null),
                ct);

            if (emailResult.IsFailed)
            {
                _logger.LogError(
                    "Email verification delivery failed. Identifier: {Identifier}",
                    PiiHasher.HashForLogging(command.Email));

                return Result.Fail(new InternalError("Failed to send the verification email."));
            }

            await uow.CommitAsync(ct);
        }
        catch (DuplicateKeyException)
        {
            return Result.Ok();
        }

        return Result.Ok();
    }
}
