using FluentResults;

namespace Pulse.BL.Feature.Email;

public class ResendEmailService : IEmailService
{
    public Task<Result> SendEmailAsync(SendEmailDto dto, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
