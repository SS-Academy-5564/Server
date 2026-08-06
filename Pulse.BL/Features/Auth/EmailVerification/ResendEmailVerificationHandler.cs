using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security;
using Pulse.BL.Features.Email;
using Pulse.DAL.Commands.EmailVerificationTokens;
using Pulse.DAL.Common.Repository;

namespace Pulse.BL.Features.Auth.EmailVerification;

/// <summary>
/// Replaces expired email verification tokens and delivers fresh verification links.
/// </summary>
public sealed class ResendEmailVerificationHandler
    : IAsyncHandler<ResendEmailVerificationCommand, Result<ResendEmailVerificationResult>>
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IEmailVerificationTokenCommands _tokenCommands;
    private readonly IEmailVerificationTokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly TimeProvider _timeProvider;
    private readonly EmailVerificationOptions _options;
    private readonly ILogger<ResendEmailVerificationHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResendEmailVerificationHandler"/> class.
    /// </summary>
    /// <param name="unitOfWorkFactory">The factory used to create the token replacement transaction.</param>
    /// <param name="tokenCommands">The email verification token persistence operations.</param>
    /// <param name="tokenService">The service used to generate and hash secure tokens.</param>
    /// <param name="emailService">The service used to deliver the replacement email.</param>
    /// <param name="timeProvider">The source of the current UTC time.</param>
    /// <param name="options">The configured token lifetime, cooldown, and verification page URL.</param>
    /// <param name="logger">The logger used to record delivery failures without sensitive values.</param>
    public ResendEmailVerificationHandler(
        IUnitOfWorkFactory unitOfWorkFactory,
        IEmailVerificationTokenCommands tokenCommands,
        IEmailVerificationTokenService tokenService,
        IEmailService emailService,
        TimeProvider timeProvider,
        IOptions<EmailVerificationOptions> options,
        ILogger<ResendEmailVerificationHandler> logger)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _tokenCommands = tokenCommands;
        _tokenService = tokenService;
        _emailService = emailService;
        _timeProvider = timeProvider;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Validates an expired token, creates its replacement, and sends a new verification email.
    /// </summary>
    /// <param name="command">The command containing the expired token.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A successful result with cooldown guidance, or a typed token-state or delivery error.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the persistence layer returns an unsupported status.</exception>
    public async Task<Result<ResendEmailVerificationResult>> HandleAsync(
        ResendEmailVerificationCommand command,
        CancellationToken ct)
    {
        string presentedTokenHash = _tokenService.ComputeHash(command.Token);
        string replacementToken = _tokenService.GenerateToken();
        string replacementTokenHash = _tokenService.ComputeHash(replacementToken);
        DateTimeOffset now = _timeProvider.GetUtcNow();

        await using IUnitOfWork uow = await _unitOfWorkFactory.CreateAsync(ct: ct);
        EmailVerificationTokenResendPreparation preparation = await _tokenCommands.PrepareResendAsync(
            new PrepareEmailVerificationTokenResendInput(
                presentedTokenHash,
                replacementTokenHash,
                now,
                now.AddHours(_options.TokenLifetimeHours),
                _options.ResendCooldownSeconds),
            ct);

        IError? error = MapFailure(preparation.Status);
        if (error is not null)
        {
            return Result.Fail<ResendEmailVerificationResult>(error);
        }

        string verificationUrl = EmailVerificationEmailBuilder.BuildVerificationUrl(
            _options.VerificationPageUrl,
            replacementToken);
        Result emailResult = await _emailService.SendEmailAsync(
            new SendEmailDto(
                To: [preparation.Email!],
                Subject: EmailVerificationEmailBuilder.BuildSubject(command.Language),
                HtmlBody: EmailVerificationEmailBuilder.BuildHtmlBody(
                    verificationUrl,
                    _options.TokenLifetimeHours,
                    command.Language),
                PlainTextBody: EmailVerificationEmailBuilder.BuildPlainTextBody(
                    verificationUrl,
                    _options.TokenLifetimeHours,
                    command.Language),
                ReplyTo: null),
            ct);

        if (emailResult.IsFailed)
        {
            _logger.LogError(
                "Replacement email verification delivery failed. Identifier: {Identifier}",
                PiiHasher.HashForLogging(preparation.Email!));

            return Result.Fail(new InternalError("Failed to send the verification email."));
        }

        await uow.CommitAsync(ct);
        return Result.Ok(new ResendEmailVerificationResult(_options.ResendCooldownSeconds));
    }

    private static IError? MapFailure(EmailVerificationTokenResendStatus status)
        => status switch
        {
            EmailVerificationTokenResendStatus.Succeeded => null,
            EmailVerificationTokenResendStatus.Invalid => new ValidationError(
                "The email verification token is invalid.",
                code: AppError.Codes.EmailVerificationTokenInvalid),
            EmailVerificationTokenResendStatus.NotExpired => new ConflictError(
                "The email verification token has not expired.",
                AppError.Codes.EmailVerificationTokenNotExpired),
            EmailVerificationTokenResendStatus.AlreadyUsed => new ConflictError(
                "The email verification token has already been used.",
                AppError.Codes.EmailVerificationTokenAlreadyUsed),
            EmailVerificationTokenResendStatus.Cooldown => new TooManyRequestsError(
                "Please wait before requesting another verification email."),
            _ => throw new InvalidOperationException($"Unsupported email verification resend status '{status}'.")
        };
}
