using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pulse.DAL.Connection;

namespace Pulse.DAL.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();

            return services;
        }
    }
}