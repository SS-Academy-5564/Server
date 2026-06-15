using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Pulse.BL.Common.Security.Tokens;

namespace Pulse.Tests.Unit.Features.Auth.Login;

public class JwtTokenGeneratorTests
{
    private readonly JwtTokenGenerator _sut;
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

        var optionsMock = new Mock<IOptions<JwtOptions>>();
        optionsMock.Setup(x => x.Value).Returns(_jwtOptions);

        _sut = new JwtTokenGenerator(optionsMock.Object);
    }

    [Fact]
    public void Generate_ShouldContainUserIdClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();

        // Act
        var token = _sut.GenerateToken(userId, roleId, organizationId, out _);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        jwtToken!.Claims
            .Should()
            .ContainSingle(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId.ToString());
    }

    [Fact]
    public void Generate_ShouldContainRoleIdClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();

        // Act
        var token = _sut.GenerateToken(userId, roleId, organizationId, out _);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        jwtToken!.Claims
            .Should()
            .ContainSingle(c => c.Type == "roleId" && c.Value == roleId.ToString());
    }

    [Fact]
    public void Generate_ShouldContainOrganizationIdClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();

        // Act
        var token = _sut.GenerateToken(userId, roleId, organizationId, out _);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        jwtToken!.Claims
            .Should()
            .ContainSingle(c => c.Type == "orgId" && c.Value == organizationId.ToString());
    }

    [Fact]
    public void Generate_ShouldSetExpiration()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var beforeGeneration = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);

        // Act
        var token = _sut.GenerateToken(userId, roleId, organizationId, out var expiresAt);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        jwtToken!.ValidTo.Should().BeCloseTo(beforeGeneration.UtcDateTime, TimeSpan.FromSeconds(5));
        expiresAt.Should().BeCloseTo(beforeGeneration, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Generate_ShouldSetIssuerAndAudience()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();

        // Act
        var token = _sut.GenerateToken(userId, roleId, organizationId, out _);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        jwtToken!.Issuer.Should().Be(_jwtOptions.Issuer);
        jwtToken!.Audiences.Should().Contain(_jwtOptions.Audience);
    }
}
