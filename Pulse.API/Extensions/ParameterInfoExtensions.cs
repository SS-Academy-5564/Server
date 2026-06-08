using System.Reflection;

namespace Pulse.API.Extensions;

public static class ParameterInfoExtensions
{
    public static bool HasCustomAttribute<TAttribute>(this ParameterInfo parameterInfo)
        where TAttribute : Attribute
    {
        return parameterInfo.CustomAttributes.Any(attribute =>
            attribute.AttributeType == typeof(TAttribute));
    }
}
