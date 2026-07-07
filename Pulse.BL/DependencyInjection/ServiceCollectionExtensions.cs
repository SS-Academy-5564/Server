using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Pulse.BL.Common.Handlers;

namespace Pulse.BL.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        Type handlerType = typeof(IAsyncHandler<,>);
        Type queryHandlerType = typeof(IAsyncQueryHandler<>);

        IEnumerable<(Type Interface, Type Implementation)> registrations = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces()
            .Where(i => i.IsGenericType &&
                (i.GetGenericTypeDefinition() == handlerType || i.GetGenericTypeDefinition() == queryHandlerType))
            .Select(i => (Interface: i, Implementation: t)));

        foreach ((Type iface, Type impl) in registrations)
        {
            services.AddScoped(iface, impl);
        }

        return services;
    }
}
