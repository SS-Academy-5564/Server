namespace Pulse.BL.Common.Helpers.Json;

public interface IJsonPathReader
{
    /// <summary>
    /// Reads a value from a JSON document at the specified path.
    /// </summary>
    /// <param name="json">The JSON document to read.</param>
    /// <param name="path">
    /// The dot-separated property path, for example <c>data.status</c>.
    /// </param>
    /// <returns>
    /// The value converted to a string, or <see langword="null"/> when the input is empty
    /// or the path does not exist.
    /// </returns>
    string? ReadValue(string json, string path);

    /// <summary>
    /// Attempts to read a value from a JSON document at the specified path.
    /// </summary>
    /// <param name="json">The JSON document to read.</param>
    /// <param name="path">
    /// The dot-separated property path, for example <c>data.status</c>.
    /// </param>
    /// <param name="value">
    /// When this method returns, contains the value converted to a string, or
    /// <see langword="null"/> when the path does not exist or the read fails.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the JSON was read successfully, including when the path
    /// does not exist; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <see cref="System.Text.Json.JsonException"/>, <see cref="InvalidOperationException"/>,
    /// and <see cref="ArgumentException"/> are treated as expected read failures and return
    /// <see langword="false"/>. Other exceptions are not suppressed and propagate to the caller.
    /// </remarks>
    bool TryReadValue(string json, string path, out string? value);
}
