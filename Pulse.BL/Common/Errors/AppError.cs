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
    public ValidationError(
        string message,
        IReadOnlyDictionary<string, string[]>? fieldErrors = null,
        string code = Codes.Validation)
        : base(message, code)
    {
        FieldErrors = fieldErrors ?? new Dictionary<string, string[]>();
    }

    public IReadOnlyDictionary<string, string[]> FieldErrors { get; }
}
public sealed class UnauthorizedError(string message) : AppError(message, Codes.Unauthorized);
public sealed class ForbiddenError(string message, string code = AppError.Codes.Forbidden) : AppError(message, code);
public sealed class ConflictError(string message, string code = AppError.Codes.Conflict) : AppError(message, code);
public sealed class InternalError(string message) : AppError(message, Codes.Internal);
public sealed class TooManyRequestsError(string message) : AppError(message, Codes.TooManyRequests);
