using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Pulse.BL.Features.Email;

public class DummyEmailService : IEmailService
{
    private readonly ILogger<DummyEmailService> _logger;

    public DummyEmailService(ILogger<DummyEmailService> logger)
    {
        _logger = logger;
    }

    public Task<Result> SendEmailAsync(
        SendEmailDto email,
        CancellationToken cancellationToken = default)
    {
        string emailJson = JsonSerializer.Serialize(email, new JsonSerializerOptions { WriteIndented = true });

        _logger.LogInformation(
            "Dummy email sent. EmailData: {EmailData}",
            emailJson);

        return Task.FromResult(Result.Ok());
    }
}
