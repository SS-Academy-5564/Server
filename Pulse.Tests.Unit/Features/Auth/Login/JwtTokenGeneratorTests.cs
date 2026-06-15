using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using Pulse.BL.Common.Security.Tokens;

namespace Pulse.Tests.Unit.Features.Auth.Login;

public class JwtTokenGeneratorTests
{
    private readonly JwtOptions _jwtOptions;

    public JwtTokenGeneratorTests()
    {
        _jwtOptions = new JwtOptions
        {
            Issuer = "https://test.local",
            Audience = "https://test.local",
            SecretKey = "super-secret-key-minimum-32-characters-long!",
            ExpirationMinutes = 60
        };
    }

    [Fact]
    public void Generate_ShouldContainUserIdClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var sut = CreateSut();

        // Act
        var token = sut.GenerateToken(userId, roleId, organizationId, out _);

        // Assert
        var jwtToken = ReadToken(token);

        jwtToken.GetClaim(JwtRegisteredClaimNames.Sub)!.Value.Should().Be(userId.ToString());
    }

    [Fact]
    public void Generate_ShouldContainRoleIdClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var sut = CreateSut();

        // Act
        var token = sut.GenerateToken(userId, roleId, organizationId, out _);

        // Assert
        var jwtToken = ReadToken(token);

        jwtToken.GetClaim("roleId")!.Value.Should().Be(roleId.ToString());
    }

    [Fact]
    public void Generate_ShouldContainOrganizationIdClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var sut = CreateSut();

        // Act
        var token = sut.GenerateToken(userId, roleId, organizationId, out _);

        // Assert
        var jwtToken = ReadToken(token);

        jwtToken.GetClaim("orgId")!.Value.Should().Be(organizationId.ToString());
    }

    [Fact]
    public void Generate_ShouldSetExpirationFromTimeProvider()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var fixedTime = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(fixedTime);
        var sut = CreateSut(timeProvider);
        var expectedExpiry = fixedTime.AddMinutes(_jwtOptions.ExpirationMinutes);

        // Act
        var token = sut.GenerateToken(userId, roleId, organizationId, out var expiresAt);

        // Assert
        var jwtToken = ReadToken(token);

        expiresAt.Should().Be(expectedExpiry);
        jwtToken.ValidTo.Should().Be(expectedExpiry.UtcDateTime);
    }

    [Fact]
    public void Generate_WhenTimeAdvances_ShouldUseUpdatedExpiration()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var initialTime = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(initialTime);
        var sut = CreateSut(timeProvider);

        timeProvider.Advance(TimeSpan.FromHours(2));
        var expectedExpiry = timeProvider.GetUtcNow().AddMinutes(_jwtOptions.ExpirationMinutes);

        // Act
        var token = sut.GenerateToken(userId, roleId, organizationId, out var expiresAt);

        // Assert
        expiresAt.Should().Be(expectedExpiry);
        ReadToken(token).ValidTo.Should().Be(expectedExpiry.UtcDateTime);
    }

    [Fact]
    public void Generate_ShouldSetIssuerAndAudience()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var sut = CreateSut();

        // Act
        var token = sut.GenerateToken(userId, roleId, organizationId, out _);

        // Assert
        var jwtToken = ReadToken(token);

        jwtToken.Issuer.Should().Be(_jwtOptions.Issuer);
        jwtToken.Audiences.Should().Contain(_jwtOptions.Audience);
    }

    private JwtTokenGenerator CreateSut(TimeProvider? timeProvider = null)
    {
        var optionsMock = new Mock<IOptions<JwtOptions>>();
        optionsMock.Setup(x => x.Value).Returns(_jwtOptions);

        return new JwtTokenGenerator(optionsMock.Object, timeProvider ?? TimeProvider.System);
    }

    private static JsonWebToken ReadToken(string token)
    {
        return new JsonWebTokenHandler().ReadJsonWebToken(token);
    }

    private sealed class FakeTimeProvider : TimeProvider
    {
        private DateTimeOffset _utcNow;

        public FakeTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public void Advance(TimeSpan duration) => _utcNow = _utcNow.Add(duration);

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }
}
