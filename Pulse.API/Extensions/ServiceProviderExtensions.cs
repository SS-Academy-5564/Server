using FluentValidation;

namespace Pulse.API.Extensions;

public static class ServiceProviderExtensions
{
    extension(IServiceProvider serviceProvider)
    {
        public IEnumerable<IValidator> GetValidators<T>()
        {
            return serviceProvider.GetValidators(typeof(T));
        }

        public IEnumerable<IValidator> GetValidators(Type type)
        {
            return serviceProvider.GetServices(typeof(IValidator<>).MakeGenericType(type)).OfType<IValidator>();
        }
    }
}
