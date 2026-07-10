namespace Pulse.BL.Common.Helpers.Json;

public interface IJsonPathReader
{
    string? ReadValue(string json, string path);
    bool TryReadValue(string json, string path, out string? value);
}
