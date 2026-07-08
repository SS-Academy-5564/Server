namespace Pulse.BL.Features.Polling.Json;

public interface IJsonPathReader
{
    string? ReadValue(string json, string path);
}
