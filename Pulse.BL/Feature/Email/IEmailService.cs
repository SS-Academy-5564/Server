using FluentResults;

namespace Pulse.BL.Feature.Email;

public interface IEmailService
{
    Task<Result> SendEmailAsync(
        SendEmailDto dto,
        CancellationToken cancellationToken = default);
}
