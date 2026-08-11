using FluentAssertions;
using Microsoft.Extensions.Options;
using Pulse.BL.Features.Polling.Options;

namespace Pulse.Tests.Unit.Features.Polling;

public class PollingOptionsValidatorTests
{
    private readonly PollingWorkerOptionsValidator _validator = new();

    [Fact]
    public void Validate_WhenOptionsAreValid_ReturnsSuccess()
    {
        // Arrange
        PollingWorkerOptions options = new()
        {
            BatchSize = 50,
            LoopIntervalSeconds = 10,
            MaxDegreeOfParallelism = 4
        };

        // Act
        ValidateOptionsResult result = _validator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenBatchSizeIsZero_ReturnsFailure()
    {
        // Arrange
        PollingWorkerOptions options = new()
        {
            BatchSize = 0,
            LoopIntervalSeconds = 10,
            MaxDegreeOfParallelism = 4
        };

        // Act
        ValidateOptionsResult result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain("PollingWorker:BatchSize must be greater than zero.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(61)]
    public void Validate_WhenLoopIntervalIsOutsideAllowedRange_ReturnsFailure(int loopIntervalSeconds)
    {
        // Arrange
        PollingWorkerOptions options = new()
        {
            BatchSize = 10,
            LoopIntervalSeconds = loopIntervalSeconds,
            MaxDegreeOfParallelism = 4
        };

        // Act
        ValidateOptionsResult result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain("PollingWorker:LoopIntervalSeconds must be between 1 and 60 seconds.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenMaxDegreeOfParallelismIsZeroOrNegative_ReturnsFailure(int maxDegreeOfParallelism)
    {
        // Arrange
        PollingWorkerOptions options = new()
        {
            BatchSize = 10,
            LoopIntervalSeconds = 10,
            MaxDegreeOfParallelism = maxDegreeOfParallelism
        };

        // Act
        ValidateOptionsResult result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain("PollingWorker:MaxDegreeOfParallelism must be greater than zero.");
    }

    [Fact]
    public void Validate_WhenMultipleValuesAreInvalid_ReturnsAllFailures()
    {
        // Arrange
        PollingWorkerOptions options = new()
        {
            BatchSize = 0,
            LoopIntervalSeconds = 100,
            MaxDegreeOfParallelism = 0
        };

        // Act
        ValidateOptionsResult result = _validator.Validate(null, options);

        // Assert
        result.Failures.Should().HaveCount(3);
    }
}
