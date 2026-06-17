using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Pulse.API.Documentation;

internal sealed class BearerSecurityOperationTransformer : IOpenApiOperationTransformer
{
    private const string SchemeId = JwtBearerDefaults.AuthenticationScheme;

    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var hasAuthorize = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<IAuthorizeData>()
            .Any();

        if (!hasAuthorize)
            return Task.CompletedTask;

        operation.Security ??= new List<OpenApiSecurityRequirement>();

        operation.Security.Add(new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference(SchemeId)] = [] });

        return Task.CompletedTask;
    }
}
