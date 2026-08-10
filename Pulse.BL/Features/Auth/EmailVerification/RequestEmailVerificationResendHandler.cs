using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security;
using Pulse.BL.Features.Email;
using Pulse.DAL.Commands.EmailVerificationTokens;
using Pulse.DAL.Common.Repository;

namespace Pulse.BL.Features.Auth.EmailVerification;

/// <summary>
/// Issues replacement verification links while keeping account state private.
/// </summary>
public sealed class RequestEmailVerificationResendHandler
    : IAsyncHandler<RequestEmailVerificationResendCommand, Result<ResendEmailVerificationResult>>
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IEmailVerificationTokenCommands _tokenCommands;
    private readonly IEmailVerificationTokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly TimeProvider _timeProvider;
    private readonly EmailVerificationOptions _options;
    private readonly ILogger<RequestEmailVerificationResendHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestEmailVerificationResendHandler"/> class.
    /// </summary>
    /// <param name="unitOfWorkFactory">The factory used to create the token replacement transaction.</param>
    /// <param name="tokenCommands">The email verification token persistence operations.</param>
    /// <param name="tokenService">The service used to generate and hash secure tokens.</param>
    /// <param name="emailService">The service used to deliver replacement emails.</param>
    /// <param name="timeProvider">The source of the current UTC time.</param>
    /// <param name="options">The configured token lifetime, cooldown, and verification page URL.</param>
    /// <param name="logger">The logger used to record delivery failures without sensitive values.</param>
    public RequestEmailVerificationResendHandler(
        IUnitOfWorkFactory unitOfWorkFactory,
        IEmailVerificationTokenCommands tokenCommands,
        IEmailVerificationTokenService tokenService,
        IEmailService emailService,
        TimeProvider timeProvider,
        IOptions<EmailVerificationOptions> options,
        ILogger<RequestEmailVerificationResendHandler> logger)
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
    /// Creates and delivers a replacement link when the account is eligible.
    /// </summary>
    /// <param name="command">The resend request containing an email address and language.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A generic successful result with cooldown guidance for every account state.</returns>
    public async Task<Result<ResendEmailVerificationResult>> HandleAsync(
        RequestEmailVerificationResendCommand command,
        CancellationToken ct)
    {
        string replacementToken = _tokenService.GenerateToken();
        string replacementTokenHash = _tokenService.ComputeHash(replacementToken);
        DateTimeOffset now = _timeProvider.GetUtcNow();

        await using IUnitOfWork uow = await _unitOfWorkFactory.CreateAsync(ct: ct);
        string? recipient = await _tokenCommands.PrepareResendByEmailAsync(
            new PrepareEmailVerificationResendByEmailInput(
                command.Email.Trim(),
                replacementTokenHash,
                now,
                now.AddHours(_options.TokenLifetimeHours),
                _options.ResendCooldownSeconds),
            ct);

        if (recipient is null)
        {
            return GenericSuccess();
        }

        string verificationUrl = EmailVerificationEmailBuilder.BuildVerificationUrl(
            _options.VerificationPageUrl,
            replacementToken);
        Result emailResult = await _emailService.SendEmailAsync(
            new SendEmailDto(
                To: [recipient],
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
                "Requested email verification delivery failed. Identifier: {Identifier}",
                PiiHasher.HashForLogging(recipient));

            return GenericSuccess();
        }

        await uow.CommitAsync(ct);
        return GenericSuccess();
    }

    private Result<ResendEmailVerificationResult> GenericSuccess()
        => Result.Ok(new ResendEmailVerificationResult(_options.ResendCooldownSeconds));
}
