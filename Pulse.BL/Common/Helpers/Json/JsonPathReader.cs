using System.Text.Json;
using System.Text.Json.Nodes;

namespace Pulse.BL.Common.Helpers.Json;

public sealed class JsonPathReader : IJsonPathReader
{
    ///<inheritdoc/>
    public bool TryReadValue(string json, string path, out string? value)
    {
        try
        {
            value = ReadValue(json, path);
            return true;
        }
        catch (Exception ex) when (
            ex is JsonException
                or InvalidOperationException
                or ArgumentException)
        {
            value = null;
            return false;
        }
    }

    ///<inheritdoc/>
    public string? ReadValue(string json, string path)
    {
        if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(path))
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
