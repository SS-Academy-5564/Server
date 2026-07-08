using System.Text.Json.Nodes;

namespace Pulse.BL.Common.Helpers.Json;

public sealed class JsonPathReader : IJsonPathReader
{
    public string? ReadValue(string json, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentNullException(nameof(path));
        }

        JsonNode? current = JsonNode.Parse(json);

        foreach (string segment in GetSegments(path))
        {
            current = current?[segment];
        }

        return current?.GetValue<string>();
    }

    private static IEnumerable<string> GetSegments(string path)
    {
        return path
            .Trim()
            .TrimStart('.')
            .Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
