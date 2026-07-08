using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Pulse.DAL.Common.Repository;
using Pulse.DAL.Connection;

namespace Pulse.DAL.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services)
    {
        services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
        services.AddCommandsAndQueriesFromAssembly(Assembly.GetExecutingAssembly());
        services.AddSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>();

        return services;
    }
}
