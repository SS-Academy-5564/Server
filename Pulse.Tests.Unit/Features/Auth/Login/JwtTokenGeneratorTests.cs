using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using Pulse.BL.Common.Security.Tokens;

namespace Pulse.Tests.Unit.Features.Auth.Login;

public class JwtTokenGeneratorTests
{
    private static readonly JsonWebTokenHandler TokenHandler = new();

    private readonly JwtOptions _jwtOptions = new()
    {
        Issuer = "https://test.local",
        Audience = "https://test.local",
        SecretKey = "super-secret-key-minimum-32-characters-long!",
        ExpirationMinutes = 60
    };

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly string RoleName = "User";
    private static readonly Guid OrganizationId = Guid.NewGuid();
    private static readonly string OrganizationName = "Test Organization";

    [Fact]
    public void Generate_ShouldContainUserIdClaim()
    {
        // Arrange
        JwtTokenGenerator sut = CreateSut();

        // Act
        GeneratedJwtToken result = sut.GenerateToken(UserId, RoleName, OrganizationId, OrganizationName);
        JsonWebToken jwtToken = ReadToken(result.Token);

        // Assert
        jwtToken.GetClaim(JwtRegisteredClaimNames.Sub)!.Value
            .Should().Be(UserId.ToString());
    }

    [Fact]
    public void Generate_ShouldContainRoleClaim()
    {
        // Arrange
        JwtTokenGenerator sut = CreateSut();

        // Act
        GeneratedJwtToken result = sut.GenerateToken(UserId, RoleName, OrganizationId, OrganizationName);
        JsonWebToken jwtToken = ReadToken(result.Token);

        // Assert
        jwtToken.GetClaim(JwtClaimNames.Role)!.Value
            .Should().Be(RoleName);
    }

    [Fact]
    public void Generate_ShouldContainOrganizationIdClaim()
    {
        // Arrange
        JwtTokenGenerator sut = CreateSut();

        // Act
        GeneratedJwtToken result = sut.GenerateToken(UserId, RoleName, OrganizationId, OrganizationName);
        JsonWebToken jwtToken = ReadToken(result.Token);

        // Assert
        jwtToken.GetClaim(JwtClaimNames.OrganizationId)!.Value
            .Should().Be(OrganizationId.ToString());
    }

    [Fact]
    public void Generate_ShouldSetExpirationFromTimeProvider()
    {
        // Arrange
        DateTimeOffset fixedTime = new(2025, 6, 15, 10, 0, 0, TimeSpan.Zero);
        FakeTimeProvider timeProvider = new(fixedTime);
        JwtTokenGenerator sut = CreateSut(timeProvider);
        DateTimeOffset expectedExpiry = fixedTime.AddMinutes(_jwtOptions.ExpirationMinutes);

        // Act
        GeneratedJwtToken generatedToken = sut.GenerateToken(UserId, RoleName, OrganizationId, OrganizationName);
        JsonWebToken jwtToken = ReadToken(generatedToken.Token);

        // Assert
        generatedToken.ExpiresAt.Should().Be(expectedExpiry);
        jwtToken.ValidTo.Should().Be(expectedExpiry.UtcDateTime);
    }

    [Fact]
    public void Generate_WhenTimeAdvances_ShouldUseUpdatedExpiration()
    {
        // Arrange
        DateTimeOffset initialTime = new(2025, 6, 15, 10, 0, 0, TimeSpan.Zero);
        FakeTimeProvider timeProvider = new(initialTime);
        JwtTokenGenerator sut = CreateSut(timeProvider);

        timeProvider.Advance(TimeSpan.FromHours(2));
        DateTimeOffset expectedExpiry = timeProvider.GetUtcNow().AddMinutes(_jwtOptions.ExpirationMinutes);

        // Act
        GeneratedJwtToken generatedToken = sut.GenerateToken(UserId, RoleName, OrganizationId, OrganizationName);
        JsonWebToken jwtToken = ReadToken(generatedToken.Token);

        // Assert
        generatedToken.ExpiresAt.Should().Be(expectedExpiry);
        jwtToken.ValidTo.Should().Be(expectedExpiry.UtcDateTime);
    }

    [Fact]
    public void Generate_ShouldSetIssuerAndAudience()
    {
        // Arrange
        JwtTokenGenerator sut = CreateSut();

        // Act
        GeneratedJwtToken result = sut.GenerateToken(UserId, RoleName, OrganizationId, OrganizationName);
        JsonWebToken jwtToken = ReadToken(result.Token);

        // Assert
        jwtToken.Issuer.Should().Be(_jwtOptions.Issuer);
        jwtToken.Audiences.Should().Contain(_jwtOptions.Audience);
    }

    private JwtTokenGenerator CreateSut(TimeProvider? timeProvider = null)
    {
        Mock<IOptions<JwtOptions>> optionsMock = new();
        optionsMock.Setup(x => x.Value).Returns(_jwtOptions);

        return new JwtTokenGenerator(
            optionsMock.Object,
            timeProvider ?? TimeProvider.System);
    }

    private static JsonWebToken ReadToken(string token)
    {
        return TokenHandler.ReadJsonWebToken(token);
    }
}
