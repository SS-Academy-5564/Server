using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Resend;

namespace Pulse.BL.Feature.Email;

public static class EmailServiceCollectionExtensions
{
    public static IServiceCollection AddEmailing(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<EmailOptions>()
            .Bind(configuration.GetRequiredSection(EmailOptions.SectionName))
            .ValidateOnStart();

        EmailOptions emailOptions = configuration
            .GetSection(EmailOptions.SectionName)
            .Get<EmailOptions>()!;

        services.AddSingleton<IValidateOptions<EmailOptions>, EmailOptionsValidator>();

        switch (emailOptions.Provider)
        {
            case EmailProvider.Dummy:
                services.AddScoped<IEmailService, DummyEmailService>();
                break;

            case EmailProvider.Resend:
                services.AddHttpClient<ResendClient>();

                services.Configure<ResendClientOptions>(options => options.ApiToken = emailOptions.ApiKey);

                services.AddTransient<IResend, ResendClient>();
                services.AddScoped<IEmailService, ResendEmailService>();
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported email provider '{emailOptions.Provider}'.");
        }

        return services;
    }
}
