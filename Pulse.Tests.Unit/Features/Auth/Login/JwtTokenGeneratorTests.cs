using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
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
        Guid userId = Guid.NewGuid();
        string roleName = "User";
        Guid organizationId = Guid.NewGuid();

        JwtTokenGenerator sut = CreateSut();

        GeneratedJwtToken result = sut.GenerateToken(userId, roleName, organizationId);
        string token = result.Token;

        JsonWebToken jwtToken = ReadToken(token);

        jwtToken.GetClaim(JwtRegisteredClaimNames.Sub)!.Value
            .Should().Be(userId.ToString());
    }

    [Fact]
    public void Generate_ShouldContainRoleClaim()
    {
        Guid userId = Guid.NewGuid();
        string roleName = "User";
        Guid organizationId = Guid.NewGuid();

        JwtTokenGenerator sut = CreateSut();

        GeneratedJwtToken result = sut.GenerateToken(userId, roleName, organizationId);
        string token = result.Token;

        JsonWebToken jwtToken = ReadToken(token);

        jwtToken.GetClaim(JwtClaimNames.Role)!.Value
            .Should().Be(roleName);
    }

    [Fact]
    public void Generate_ShouldContainOrganizationIdClaim()
    {
        Guid userId = Guid.NewGuid();
        string roleName = "User";
        Guid organizationId = Guid.NewGuid();

        JwtTokenGenerator sut = CreateSut();

        GeneratedJwtToken result = sut.GenerateToken(userId, roleName, organizationId);
        string token = result.Token;

        JsonWebToken jwtToken = ReadToken(token);

        jwtToken.GetClaim(JwtClaimNames.OrganizationId)!.Value
            .Should().Be(organizationId.ToString());
    }

    [Fact]
    public void Generate_ShouldSetExpirationFromTimeProvider()
    {
        Guid userId = Guid.NewGuid();
        string roleName = "User";
        Guid organizationId = Guid.NewGuid();

        DateTimeOffset fixedTime = new(2025, 6, 15, 10, 0, 0, TimeSpan.Zero);

        FakeTimeProvider timeProvider = new(fixedTime);
        JwtTokenGenerator sut = CreateSut(timeProvider);

        DateTimeOffset expectedExpiry =
            fixedTime.AddMinutes(_jwtOptions.ExpirationMinutes);

        GeneratedJwtToken generatedToken =
            sut.GenerateToken(userId, roleName, organizationId);

        JsonWebToken jwtToken = ReadToken(generatedToken.Token);

        generatedToken.ExpiresAt.Should().Be(expectedExpiry);
        jwtToken.ValidTo.Should().Be(expectedExpiry.UtcDateTime);
    }

    [Fact]
    public void Generate_WhenTimeAdvances_ShouldUseUpdatedExpiration()
    {
        Guid userId = Guid.NewGuid();
        string roleName = "User";
        Guid organizationId = Guid.NewGuid();

        DateTimeOffset initialTime = new(2025, 6, 15, 10, 0, 0, TimeSpan.Zero);

        FakeTimeProvider timeProvider = new(initialTime);
        JwtTokenGenerator sut = CreateSut(timeProvider);

        timeProvider.Advance(TimeSpan.FromHours(2));

        DateTimeOffset expectedExpiry =
            timeProvider.GetUtcNow().AddMinutes(_jwtOptions.ExpirationMinutes);

        GeneratedJwtToken generatedToken =
            sut.GenerateToken(userId, roleName, organizationId);

        JsonWebToken jwtToken = ReadToken(generatedToken.Token);

        generatedToken.ExpiresAt.Should().Be(expectedExpiry);
        jwtToken.ValidTo.Should().Be(expectedExpiry.UtcDateTime);
    }

    [Fact]
    public void Generate_ShouldSetIssuerAndAudience()
    {
        Guid userId = Guid.NewGuid();
        string roleName = "User";
        Guid organizationId = Guid.NewGuid();

        JwtTokenGenerator sut = CreateSut();

        GeneratedJwtToken result = sut.GenerateToken(userId, roleName, organizationId);
        string token = result.Token;

        JsonWebToken jwtToken = ReadToken(token);

        jwtToken.Issuer.Should().Be(_jwtOptions.Issuer);
        jwtToken.Audiences.Should().Contain(_jwtOptions.Audience);
    }

    private JwtTokenGenerator CreateSut(TimeProvider? timeProvider = null)
    {
        Mock<IOptions<JwtOptions>> optionsMock = new();
        optionsMock.Setup(x => x.Value).Returns(_jwtOptions);

        return new JwtTokenGenerator(
            optionsMock.Object,
            timeProvider ?? TimeProvider.System
        );
    }

    private static JsonWebToken ReadToken(string token)
    {
        JsonWebTokenHandler handler = new();
        return handler.ReadJsonWebToken(token);
    }
}
