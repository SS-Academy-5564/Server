using FluentResults;

namespace Pulse.BL.Common.Errors;

public static class AppErrors
{
    public static Result Fail(AppError error) => Result.Fail(error);
    public static Result<T> Fail<T>(AppError error) => Result.Fail<T>(error);
}
