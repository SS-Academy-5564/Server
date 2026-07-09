namespace Pulse.DAL.Common.Constants;

public static class RequestStatusNames
{
    public const string Success = nameof(Success);
    public const string Failed = nameof(Failed);
    public const string Timeout = nameof(Timeout);
    public const string NetworkError = nameof(NetworkError);
    public const string ExtractionError = nameof(ExtractionError);
    public const string UnexpectedError = nameof(UnexpectedError);
}
