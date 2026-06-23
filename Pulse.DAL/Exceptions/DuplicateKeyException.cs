namespace Pulse.DAL.Exceptions;

public class DuplicateKeyException : Exception
{
    public string FieldName { get; }
    public DuplicateKeyException(string fieldName)
        : base($"Duplicate value for field '{fieldName}'.")
    {
        FieldName = fieldName;
    }
}
