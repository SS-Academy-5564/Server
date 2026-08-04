using FluentResults;

namespace Pulse.BL.Common.Errors;

public abstract class AppError : Error
{
    protected AppError(string message, string code) : base(message)
    {
        Code = code;
    }

    public string Code { get; }

    public static class Codes
    {
        public const string NotFound = "NOT_FOUND";
        public const string Validation = "VALIDATION_ERROR";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string Forbidden = "FORBIDDEN";
        public const string Conflict = "CONFLICT";
        public const string TooManyRequests = "TOO_MANY_REQUESTS";
        public const string Internal = "INTERNAL_ERROR";
        public const string EmailVerificationTokenInvalid = "EMAIL_VERIFICATION_TOKEN_INVALID";
        public const string EmailVerificationTokenExpired = "EMAIL_VERIFICATION_TOKEN_EXPIRED";
        public const string EmailVerificationTokenAlreadyUsed = "EMAIL_VERIFICATION_TOKEN_ALREADY_USED";
        public const string EmailNotVerified = "EMAIL_NOT_VERIFIED";
    }
}

public sealed class NotFoundError(string message) : AppError(message, Codes.NotFound);
public sealed class ValidationError : AppError
{
    public ValidationError(string message, IReadOnlyDictionary<string, string[]>? fieldErrors = null)
        : base(message, Codes.Validation)
    {
        FieldErrors = fieldErrors ?? new Dictionary<string, string[]>();
    }

    public IReadOnlyDictionary<string, string[]> FieldErrors { get; }
}
public sealed class UnauthorizedError(string message) : AppError(message, Codes.Unauthorized);
public sealed class ForbiddenError(string message) : AppError(message, Codes.Forbidden);
public sealed class ConflictError(string message) : AppError(message, Codes.Conflict);
public sealed class InternalError(string message) : AppError(message, Codes.Internal);
public sealed class TooManyRequestsError(string message) : AppError(message, Codes.TooManyRequests);

/// <summary>
/// Represents an email verification token that does not exist.
/// </summary>
public sealed class InvalidEmailVerificationTokenError : AppError
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidEmailVerificationTokenError"/> class.
    /// </summary>
    public InvalidEmailVerificationTokenError()
        : base("The email verification token is invalid.", Codes.EmailVerificationTokenInvalid)
    {
    }
}

/// <summary>
/// Represents an email verification token whose lifetime has elapsed.
/// </summary>
public sealed class ExpiredEmailVerificationTokenError : AppError
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExpiredEmailVerificationTokenError"/> class.
    /// </summary>
    public ExpiredEmailVerificationTokenError()
        : base("The email verification token has expired.", Codes.EmailVerificationTokenExpired)
    {
    }
}

/// <summary>
/// Represents an email verification token that has already been consumed.
/// </summary>
public sealed class AlreadyUsedEmailVerificationTokenError : AppError
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AlreadyUsedEmailVerificationTokenError"/> class.
    /// </summary>
    public AlreadyUsedEmailVerificationTokenError()
        : base("The email verification token has already been used.", Codes.EmailVerificationTokenAlreadyUsed)
    {
    }
}

/// <summary>
/// Represents an account that cannot authenticate until its email address is verified.
/// </summary>
public sealed class EmailNotVerifiedError : AppError
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailNotVerifiedError"/> class.
    /// </summary>
    public EmailNotVerifiedError()
        : base("Please verify your email address to continue.", Codes.EmailNotVerified)
    {
    }
}
