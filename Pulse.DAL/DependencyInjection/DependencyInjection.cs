using Microsoft.Extensions.DependencyInjection;
using Pulse.DAL.Connection;
using Pulse.DAL.Database;

namespace Pulse.DAL.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services)
    {
        services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();

        return services;
    }

    public static IServiceCollection AddDatabaseMigrations(this IServiceCollection services)
    {
        services.AddHostedService<DatabaseMigration>();

        return services;
    }
}