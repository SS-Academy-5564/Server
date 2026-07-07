namespace Pulse.BL.Common.Handlers;

public interface IAsyncQuery<TResult>
{
    Task<TResult> HandleAsync(CancellationToken ct = default);
}