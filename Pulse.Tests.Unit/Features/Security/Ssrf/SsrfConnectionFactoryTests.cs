using System.Net;
using System.Net.Http;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Security.Ssrf;
using Pulse.BL.Features.Polling.Http;

namespace Pulse.Tests.Unit.Features.Security.Ssrf;

public class SsrfConnectionFactoryTests
{
    private sealed class StubDnsResolver : IDnsResolver
    {
        private readonly IPAddress[] _addresses;

        public StubDnsResolver(params IPAddress[] addresses) => _addresses = addresses;

        public int CallCount { get; private set; }

        public Task<IPAddress[]> ResolveAsync(string host, CancellationToken ct)
        {
            CallCount++;
            return Task.FromResult(_addresses);
        }
    }

    private static SsrfGuard CreateGuard(SsrfProtectionOptions? options = null)
        => new(Options.Create(options ?? new SsrfProtectionOptions()));

    [Fact]
    public async Task ResolveAndValidateAsync_WhenHostResolvesToInternal_ThrowsAsync()
    {
        StubDnsResolver dns = new(IPAddress.Parse("169.254.169.254"));
        SsrfConnectionFactory factory = new(CreateGuard(), dns);

        Func<Task> act = () => factory.ResolveAndValidateAsync("metadata.evil.test", CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task ResolveAndValidateAsync_WhenAnyResolvedAddressIsInternal_ThrowsAsync()
    {
        // Public + internal (DNS-rebinding style multi-answer) must be rejected.
        StubDnsResolver dns = new(IPAddress.Parse("8.8.8.8"), IPAddress.Parse("10.0.0.5"));
        SsrfConnectionFactory factory = new(CreateGuard(), dns);

        Func<Task> act = () => factory.ResolveAndValidateAsync("rebind.evil.test", CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task ResolveAndValidateAsync_WhenHostResolvesToPublic_ReturnsAddressesAsync()
    {
        StubDnsResolver dns = new(IPAddress.Parse("93.184.216.34"));
        SsrfConnectionFactory factory = new(CreateGuard(), dns);

        IReadOnlyList<IPAddress> addresses =
            await factory.ResolveAndValidateAsync("example.com", CancellationToken.None);

        addresses.Should().ContainSingle().Which.Should().Be(IPAddress.Parse("93.184.216.34"));
    }

    [Fact]
    public async Task ResolveAndValidateAsync_WhenHostIsInternalIpLiteral_DoesNotResolveDnsAndThrowsAsync()
    {
        StubDnsResolver dns = new(IPAddress.Parse("8.8.8.8"));
        SsrfConnectionFactory factory = new(CreateGuard(), dns);

        Func<Task> act = () => factory.ResolveAndValidateAsync("127.0.0.1", CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
        dns.CallCount.Should().Be(0);
    }
}
