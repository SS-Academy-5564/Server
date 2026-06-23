using FluentResults;

namespace Pulse.BL.Features.Email;

public interface IEmailService
{
    Task<Result> SendEmailAsync(
        SendEmailDto dto,
        CancellationToken cancellationToken = default);
}
