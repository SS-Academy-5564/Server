using FluentAssertions;
using Pulse.BL.Features.Auth.EmailVerification;

namespace Pulse.Tests.Unit.Features.Auth.EmailVerification;

public class EmailVerificationTokenServiceTests
{
    private readonly EmailVerificationTokenService _service = new();

    [Fact]
    public void GenerateToken_CalledTwice_ReturnsDistinctUrlSafeTokens()
    {
        string first = _service.GenerateToken();
        string second = _service.GenerateToken();

        first.Should().HaveLength(43);
        second.Should().HaveLength(43);
        first.Should().NotBe(second);
        first.Should().MatchRegex("^[A-Za-z0-9_-]+$");
        second.Should().MatchRegex("^[A-Za-z0-9_-]+$");
    }

    [Fact]
    public void ComputeHash_SameToken_ReturnsStableSha256WithoutRawToken()
    {
        const string token = "verification-token";

        string first = _service.ComputeHash(token);
        string second = _service.ComputeHash(token);

        first.Should().Be(second);
        first.Should().HaveLength(64);
        first.Should().NotContain(token);
    }
}
