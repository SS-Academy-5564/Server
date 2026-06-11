using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Pulse.BL.Feature.Email;

public class EmailOptionsValidator : IValidateOptions<EmailOptions>
{
    public ValidateOptionsResult Validate(string? name, EmailOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var errors = new List<string>();

        if (options.Provider == EmailProvider.None)
        {
            errors.Add("Email:Provider is required.");
        }
        else if (!Enum.IsDefined(options.Provider))
        {
            errors.Add("Email:Provider must be a valid provider.");
        }

        if (string.IsNullOrWhiteSpace(options.FromAddress))
        {
            errors.Add("Email:FromAddress is required.");
        }
        else if (!IsValidEmail(options.FromAddress))
        {
            errors.Add("Email:FromAddress must be a valid email address.");
        }

        if (string.IsNullOrWhiteSpace(options.FromName))
        {
            errors.Add("Email:FromName is required.");
        }

        if (options.Provider == EmailProvider.Resend &&
            string.IsNullOrWhiteSpace(options.ApiKey))
        {
            errors.Add("Email:ApiKey is required when Email:Provider is 'Resend'.");
        }

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }

    private static bool IsValidEmail(string email)
    {
        return MailAddress.TryCreate(email, out _);
    }
}
