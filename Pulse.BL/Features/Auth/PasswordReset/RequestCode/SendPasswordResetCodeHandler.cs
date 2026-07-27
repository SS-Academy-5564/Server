using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security;
using Pulse.BL.Common.Security.Passwords;
using Pulse.BL.Features.Auth.PasswordReset;
using Pulse.BL.Features.Email;
using Pulse.DAL.Commands.PasswordResetCodes;
using Pulse.DAL.Queries.Users;

namespace Pulse.BL.Features.Auth.PasswordReset.RequestCode;

/// <summary>
/// Handles password reset code requests with localized email notifications.
/// Always returns success to prevent email enumeration attacks.
/// Emails are sent in the requested language (with English as fallback).
/// </summary>
public class SendPasswordResetCodeHandler : IAsyncHandler<SendPasswordResetCodeCommand, Result>
{
    private readonly IUserQueries _userQueries;
    private readonly IPasswordResetCodeCommands _codeCommands;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly TimeProvider _timeProvider;
    private readonly PasswordResetOptions _options;
    private readonly ILogger<SendPasswordResetCodeHandler> _logger;

    public SendPasswordResetCodeHandler(
        IUserQueries userQueries,
        IPasswordResetCodeCommands codeCommands,
        IPasswordHasher passwordHasher,
        IEmailService emailService,
        TimeProvider timeProvider,
        IOptions<PasswordResetOptions> options,
        ILogger<SendPasswordResetCodeHandler> logger)
    {
        _userQueries = userQueries;
        _codeCommands = codeCommands;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _timeProvider = timeProvider;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Processes a password reset request by generating a 6-digit OTP code and sending it via email.
    /// Always returns success (even for non-existent emails) to prevent email enumeration attacks.
    /// </summary>
    /// <param name="command">The password reset request with email and desired language.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>Always returns a successful result.</returns>
    public async Task<Result> HandleAsync(SendPasswordResetCodeCommand command, CancellationToken ct)
    {
        // Fallback to English if language is unsupported (should not happen due to EmailLanguageResolver, but defensive)
        string language = command.IsLanguageSupported() ? command.Language : PasswordResetConstants.SupportedLanguages.English;

        Guid? userId = await _userQueries.GetIdByEmailAsync(command.Email, ct);

        // Always return Ok to prevent email enumeration
        if (userId is null)
        {
            _logger.LogInformation(
                "Password reset requested for non-existent email. Identifier: {Identifier}",
                PiiHasher.HashForLogging(command.Email));
            return Result.Ok();
        }

        // Generate 6-digit OTP
        string plainCode = GenerateSixDigitCode();
        string codeHash = _passwordHasher.HashPassword(plainCode);
        DateTimeOffset expiresAt = _timeProvider.GetUtcNow().AddMinutes(_options.CodeTtlMinutes);

        // Send the email first
        Result emailResult = await _emailService.SendEmailAsync(new SendEmailDto(
            To: [command.Email],
            Subject: PasswordResetEmailBuilder.BuildSubject(language),
            HtmlBody: PasswordResetEmailBuilder.BuildHtmlBody(plainCode, _options.CodeTtlMinutes, language),
            PlainTextBody: PasswordResetEmailBuilder.BuildPlainTextBody(plainCode, _options.CodeTtlMinutes, language),
            ReplyTo: null), ct);

        if (emailResult.IsFailed)
        {
            _logger.LogError("Failed to send reset email for identifier: {Identifier}",
                PiiHasher.HashForLogging(command.Email));

            return Result.Ok();
        }

        // Transactionally replace any existing codes for this user with a fresh one ONLY after successful email
        await _codeCommands.ReplaceAsync(new PasswordResetCodeInput(userId.Value, codeHash, expiresAt), ct);

        _logger.LogInformation(
            "Password reset code issued. Identifier: {Identifier}",
            PiiHasher.HashForLogging(command.Email));

        return Result.Ok();
    }

    private static string GenerateSixDigitCode()
    {
        // Cryptographically random 6-digit code (000000–999999)
        int code = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 1_000_000);
        return code.ToString("D6");
    }
}
