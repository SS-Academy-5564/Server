using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Security.Ssrf;

namespace Pulse.Tests.Unit.Features.Security.Ssrf;

public class SsrfGuardTests
{
    private static SsrfGuard CreateGuard(SsrfProtectionOptions? options = null)
        => new(Options.Create(options ?? new SsrfProtectionOptions()));

    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("127.10.20.30")]
    [InlineData("10.0.0.1")]
    [InlineData("172.16.5.4")]
    [InlineData("172.31.255.255")]
    [InlineData("192.168.1.1")]
    [InlineData("169.254.1.1")]
    [InlineData("169.254.169.254")]
    [InlineData("0.0.0.0")]
    [InlineData("::")]
    [InlineData("::1")]
    [InlineData("fc00::1")]
    [InlineData("fd12:3456::1")]
    [InlineData("fe80::1")]
    [InlineData("::ffff:127.0.0.1")]
    public void IsAddressAllowed_InternalAddress_ReturnsFalse(string ip)
    {
        SsrfGuard guard = CreateGuard();

        guard.IsAddressAllowed(IPAddress.Parse(ip)).Should().BeFalse();
    }

    [Theory]
    [InlineData("8.8.8.8")]
    [InlineData("1.1.1.1")]
    [InlineData("93.184.216.34")]
    [InlineData("2606:4700:4700::1111")]
    public void IsAddressAllowed_PublicAddress_ReturnsTrue(string ip)
    {
        SsrfGuard guard = CreateGuard();

        guard.IsAddressAllowed(IPAddress.Parse(ip)).Should().BeTrue();
    }

    [Fact]
    public void IsAddressAllowed_WhenPrivateNetworksAllowed_ReturnsTrueForInternal()
    {
        SsrfGuard guard = CreateGuard(new SsrfProtectionOptions { AllowPrivateNetworks = true });

        guard.IsAddressAllowed(IPAddress.Parse("127.0.0.1")).Should().BeTrue();
        guard.IsAddressAllowed(IPAddress.Parse("10.0.0.1")).Should().BeTrue();
    }

    [Fact]
    public void IsAddressAllowed_WhenAddressInAllowedCidr_ReturnsTrue()
    {
        SsrfGuard guard = CreateGuard(new SsrfProtectionOptions
        {
            AllowedCidrs = ["10.10.0.0/16"]
        });

        guard.IsAddressAllowed(IPAddress.Parse("10.10.5.5")).Should().BeTrue();
        // Outside the allow range remains blocked.
        guard.IsAddressAllowed(IPAddress.Parse("10.20.5.5")).Should().BeFalse();
    }

    [Fact]
    public void IsAddressAllowed_WhenAddressInExtraBlockedCidr_ReturnsFalse()
    {
        SsrfGuard guard = CreateGuard(new SsrfProtectionOptions
        {
            BlockedCidrs = ["203.0.113.0/24"]
        });

        guard.IsAddressAllowed(IPAddress.Parse("203.0.113.7")).Should().BeFalse();
    }

    [Theory]
    [InlineData("localhost")]
    [InlineData("LOCALHOST")]
    [InlineData("service.localhost")]
    [InlineData("127.0.0.1")]
    [InlineData("169.254.169.254")]
    [InlineData("[::1]")]
    public void TryValidateHost_InternalHost_ReturnsFalse(string host)
    {
        SsrfGuard guard = CreateGuard();

        bool result = guard.TryValidateHost(host, out string? error);

        result.Should().BeFalse();
        error.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("example.com")]
    [InlineData("api.example.com")]
    [InlineData("8.8.8.8")]
    public void TryValidateHost_PublicHost_ReturnsTrue(string host)
    {
        SsrfGuard guard = CreateGuard();

        bool result = guard.TryValidateHost(host, out string? error);

        result.Should().BeTrue();
        error.Should().BeNull();
    }

    [Fact]
    public void TryValidateHost_WhenPrivateNetworksAllowed_AllowsInternalLiteral()
    {
        SsrfGuard guard = CreateGuard(new SsrfProtectionOptions { AllowPrivateNetworks = true });

        guard.TryValidateHost("127.0.0.1", out _).Should().BeTrue();
    }
}
