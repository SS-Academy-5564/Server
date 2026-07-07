namespace Pulse.BL.Common.Handlers;

public interface IAsyncQueryHandler<TResult>
{
    Task<TResult> HandleAsync(CancellationToken ct = default);
}
