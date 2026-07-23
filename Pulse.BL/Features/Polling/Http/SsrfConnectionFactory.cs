using System.Net;
using System.Net.Sockets;
using Pulse.BL.Common.Security.Ssrf;

namespace Pulse.BL.Features.Polling.Http;

/// <summary>
/// Provides a <see cref="SocketsHttpHandler.ConnectCallback"/> that resolves the
/// destination host and validates every resolved IP against <see cref="ISsrfGuard"/>
/// before opening a socket. Because validation happens at connection time on
/// every request, it defeats DNS rebinding — a host that resolved to a public IP
/// at create-time is re-checked at poll-time.
/// </summary>
public sealed class SsrfConnectionFactory
{
    private readonly ISsrfGuard _guard;
    private readonly IDnsResolver _dnsResolver;

    public SsrfConnectionFactory(ISsrfGuard guard, IDnsResolver dnsResolver)
    {
        _guard = guard;
        _dnsResolver = dnsResolver;
    }

    /// <summary>
    /// Connects to the requested endpoint, validating the resolved address first.
    /// Throws <see cref="HttpRequestException"/> when the destination is blocked,
    /// which the caller records as a failed poll.
    /// </summary>
    public async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext context, CancellationToken ct)
    {
        DnsEndPoint endPoint = context.DnsEndPoint;
        IReadOnlyList<IPAddress> addresses = await ResolveAndValidateAsync(endPoint.Host, ct);

        Exception? lastError = null;

        foreach (IPAddress address in addresses)
        {
            Socket socket = new(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };

            try
            {
                await socket.ConnectAsync(new IPEndPoint(address, endPoint.Port), ct);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch (OperationCanceledException)
            {
                socket.Dispose();
                throw;
            }
            catch (Exception ex)
            {
                socket.Dispose();
                lastError = ex;
            }
        }

        throw lastError ?? new HttpRequestException($"Host '{endPoint.Host}' could not be connected to.");
    }

    /// <summary>
    /// Resolves the host and returns its addresses only if every resolved address
    /// is permitted; otherwise throws <see cref="HttpRequestException"/>.
    /// </summary>
    public async Task<IReadOnlyList<IPAddress>> ResolveAndValidateAsync(string host, CancellationToken ct)
    {
        IPAddress[] addresses = IPAddress.TryParse(host, out IPAddress? literal)
            ? [literal]
            : await _dnsResolver.ResolveAsync(host, ct);

        if (addresses.Length == 0)
        {
            throw new HttpRequestException($"Host '{host}' did not resolve to any address.");
        }

        foreach (IPAddress address in addresses)
        {
            if (!_guard.IsAddressAllowed(address))
            {
                throw new HttpRequestException("Destination address is not permitted.");
            }
        }

        return addresses;
    }
}
