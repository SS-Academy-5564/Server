using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
<<<<<<< HEAD
=======
using Microsoft.Extensions.Options;
>>>>>>> 33f6239 (feat: configure email service with options and validation)
using Pulse.BL.Feature.Email;

namespace Pulse.BL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEmailing(configuration);

        return services;
    }
}
