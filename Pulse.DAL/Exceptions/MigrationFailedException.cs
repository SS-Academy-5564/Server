namespace Pulse.DAL.Exceptions;

public sealed class MigrationFailedException : Exception
{
    public MigrationFailedException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}
