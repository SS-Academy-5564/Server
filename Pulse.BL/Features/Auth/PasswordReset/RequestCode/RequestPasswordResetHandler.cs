using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Security;
using Pulse.BL.Common.Security.Passwords;
using Pulse.BL.Features.Email;
using Pulse.DAL.Commands.PasswordResetCodes;
using Pulse.DAL.Queries.Users;

namespace Pulse.BL.Features.Auth.PasswordReset.RequestCode;

/// <inheritdoc/>
public class RequestPasswordResetHandler : IRequestPasswordResetHandler
{
    private readonly IUserQueries _userQueries;
    private readonly IPasswordResetCodeCommands _codeCommands;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly TimeProvider _timeProvider;
    private readonly PasswordResetOptions _options;
    private readonly ILogger<RequestPasswordResetHandler> _logger;

    public RequestPasswordResetHandler(
        IUserQueries userQueries,
        IPasswordResetCodeCommands codeCommands,
        IPasswordHasher passwordHasher,
        IEmailService emailService,
        TimeProvider timeProvider,
        IOptions<PasswordResetOptions> options,
        ILogger<RequestPasswordResetHandler> logger)
    {
        _userQueries = userQueries;
        _codeCommands = codeCommands;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _timeProvider = timeProvider;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result> RequestAsync(RequestPasswordResetCommand command, CancellationToken ct)
    {
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
            Subject: PasswordResetEmailBuilder.BuildSubject(),
            HtmlBody: PasswordResetEmailBuilder.BuildHtmlBody(plainCode, _options.CodeTtlMinutes),
            PlainTextBody: PasswordResetEmailBuilder.BuildPlainTextBody(plainCode, _options.CodeTtlMinutes),
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
