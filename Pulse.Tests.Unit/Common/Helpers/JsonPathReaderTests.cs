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
    public void ReadValue_WhenPathIsEmpty_ThrowsArgumentNullException()
    {
        // Arrange
        JsonPathReader reader = new();
        string json = "{}";

        // Act
        string? result = reader.ReadValue(json, "");

        // Assert
        result.Should().BeNull();
    }
}
