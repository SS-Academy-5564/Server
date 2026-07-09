using System.Text.Json.Nodes;

namespace Pulse.BL.Common.Helpers.Json;

public sealed class JsonPathReader : IJsonPathReader
{
    public string? ReadValue(string json, string path)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        JsonNode? current = JsonNode.Parse(json);

        foreach (string segment in GetSegments(path))
        {
            current = current?[segment];
        }

        return current switch
        {
            null => null,
            JsonValue value when value.TryGetValue(out string? s) => s,
            JsonValue value => value.ToJsonString(),
            _ => current.ToString()
        };
    }

    /// <summary>
    /// Splits a dot-separated path into individual path segments.
    /// </summary>
    /// <param name="path">
    /// Dot-separated path, for example <c>parent.child.value</c>.
    /// Leading dots and surrounding whitespace are ignored.
    /// </param>
    /// <returns>
    /// Path segments in order, for example <c>["parent", "child", "value"]</c>.
    /// </returns>
    public IEnumerable<string> GetSegments(string path)
    {
        return path
            .Trim()
            .TrimStart('.')
            .Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
