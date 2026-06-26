namespace Pulse.BL.Common.Handlers;

public interface IAsyncHandler<in TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken ct);
}
