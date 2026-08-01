using FluentAssertions;
using Pulse.BL.Common.Security.Tokens;

namespace Pulse.Tests.Unit.Common.Security.Tokens;

public class RefreshTokenServiceTests
{
    private readonly RefreshTokenService _sut;

    public RefreshTokenServiceTests()
    {
        _sut = new RefreshTokenService();
    }

    [Fact]
    public void GenerateToken_ReturnsNonEmptyString()
    {
        // Act
        string token = _sut.GenerateToken();

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateToken_SuccessiveCallsReturnDifferentTokens()
    {
        // Act
        string token1 = _sut.GenerateToken();
        string token2 = _sut.GenerateToken();

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void ComputeHash_ReturnsConsistentHashForSameInput()
    {
        // Arrange
        string input = "test_token_123";

        // Act
        string hash1 = _sut.ComputeHash(input);
        string hash2 = _sut.ComputeHash(input);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void ComputeHash_ReturnsDifferentHashForDifferentInput()
    {
        // Arrange
        string input1 = "test_token_123";
        string input2 = "test_token_124";

        // Act
        string hash1 = _sut.ComputeHash(input1);
        string hash2 = _sut.ComputeHash(input2);

        // Assert
        hash1.Should().NotBe(hash2);
    }
}
