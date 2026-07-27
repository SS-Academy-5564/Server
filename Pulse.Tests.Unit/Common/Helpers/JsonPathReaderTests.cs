using FluentAssertions;
using Pulse.BL.Common.Helpers.Json;

namespace Pulse.Tests.Unit.Common.Helpers;

public class JsonPathReaderTests
{
    [Fact]
    public void ReadValue_WhenPathExistsAtRoot_ReturnsValue()
    {
        // Arrange
        JsonPathReader reader = new();
        string json =
            """
            {
              "status": "ok"
            }
            """;

        // Act
        string? result = reader.ReadValue(json, "status");

        // Assert
        result.Should().Be("ok");
    }

    [Fact]
    public void ReadValue_WhenPathExistsNested_ReturnsValue()
    {
        // Arrange
        JsonPathReader reader = new();
        string json =
            """
            {
              "data": {
                "status": "healthy"
              }
            }
            """;

        // Act
        string? result = reader.ReadValue(json, "data.status");

        // Assert
        result.Should().Be("healthy");
    }

    [Fact]
    public void ReadValue_WhenPathStartsWithDot_ReturnsValue()
    {
        // Arrange
        JsonPathReader reader = new();
        string json =
            """
            {
              "data": {
                "status": "healthy"
              }
            }
            """;

        // Act
        string? result = reader.ReadValue(json, ".data.status");

        // Assert
        result.Should().Be("healthy");
    }

    [Fact]
    public void ReadValue_WhenPathDoesNotExist_ReturnsNull()
    {
        // Arrange
        JsonPathReader reader = new();
        string json =
            """
            {
              "status": "ok"
            }
            """;

        // Act
        string? result = reader.ReadValue(json, "data.status");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ReadValue_WhenValueIsNumber_ReturnsJsonValue()
    {
        // Arrange
        JsonPathReader reader = new();
        string json =
            """
            {
              "data": {
                "value": 42
              }
            }
            """;

        // Act
        string? result = reader.ReadValue(json, "data.value");

        // Assert
        result.Should().Be("42");
    }

    [Fact]
    public void ReadValue_WhenPathIsEmpty_ReturnsNull()
    {
        // Arrange
        JsonPathReader reader = new();
        string json = "{}";

        // Act
        string? result = reader.ReadValue(json, "");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void TryReadValue_WhenJsonIsMalformed_ReturnsFalseAndNullValue()
    {
        // Arrange
        JsonPathReader reader = new();
        string json = """
                      {
                        "status":"ok
                      """;

        // Act
        bool result = reader.TryReadValue(json, "status", out string? value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void TryReadValue_WhenPathWalksIntoValue_ReturnsFalseAndNullValue()
    {
        // Arrange
        JsonPathReader reader = new();
        string json =
            """
            {
              "data": "healthy"
            }
            """;

        // Act
        bool result = reader.TryReadValue(json, "data.status", out string? value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void TryReadValue_WhenPathDoesNotExist_ReturnsTrueAndNullValue()
    {
        // Arrange
        JsonPathReader reader = new();
        string json =
            """
            {
              "status": "ok"
            }
            """;

        // Act
        bool result = reader.TryReadValue(json, "data.status", out string? value);

        // Assert
        result.Should().BeTrue();
        value.Should().BeNull();
    }

    [Fact]
    public void TryReadValue_WhenPathExists_ReturnsTrueAndValue()
    {
        // Arrange
        JsonPathReader reader = new();
        string json =
            """
            {
              "status": "ok"
            }
            """;

        // Act
        bool result = reader.TryReadValue(json, "status", out string? value);

        // Assert
        result.Should().BeTrue();
        value.Should().Be("ok");
    }
}
