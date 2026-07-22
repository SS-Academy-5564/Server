using FluentResults;
using Pulse.BL.Common.Handlers;
using Pulse.DAL.Common.Constants;

namespace Pulse.BL.Features.DefaultOrganization;

public sealed class GetDefaultOrganizationHandler
    : IAsyncQueryHandler<Result<GetDefaultOrganizationResult>>
{
    public Task<Result<GetDefaultOrganizationResult>> HandleAsync(CancellationToken ct = default)
    {
        GetDefaultOrganizationResult result =
            new(SeededIds.Organizations.Default);

        return Task.FromResult(Result.Ok(result));
    }
}
