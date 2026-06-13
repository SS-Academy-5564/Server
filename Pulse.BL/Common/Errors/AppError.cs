using FluentResults;

namespace Pulse.BL.Common.Errors;

public abstract record AppError(string Message, string Code) : IError
{
    public List<IError> Reasons { get; } = new();
    public Dictionary<string, object> Metadata { get; } = new();

    public static class Codes
    {
        public const string NotFound = "NOT_FOUND";
        public const string Validation = "VALIDATION_ERROR";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string Forbidden = "FORBIDDEN";
        public const string Conflict = "CONFLICT";
        public const string Internal = "INTERNAL_ERROR";
    }
}

public record NotFoundError(string Message) : AppError(Message, Codes.NotFound);
public record ValidationError(string Message, IReadOnlyDictionary<string, string[]>? FieldErrors = null)
    : AppError(Message, Codes.Validation);
public record UnauthorizedError(string Message) : AppError(Message, Codes.Unauthorized);
public record ForbiddenError(string Message) : AppError(Message, Codes.Forbidden);
public record ConflictError(string Message) : AppError(Message, Codes.Conflict);
public record InternalError(string Message) : AppError(Message, Codes.Internal);
