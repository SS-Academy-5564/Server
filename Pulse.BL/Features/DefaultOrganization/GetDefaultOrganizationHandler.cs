using FluentResults;
using Pulse.DAL.Common.Constants;

namespace Pulse.BL.Features.DefaultOrganization;

public class GetDefaultOrganizationHandler
{
    public Task<Result<GetDefaultOrganizationResult>> HandleAsync(GetDefaultOrganizationQuery query, CancellationToken ct)
    {
        _ = query;
        _ = ct;

        GetDefaultOrganizationResult result =
            new(SeededIds.Organizations.Default);

        return Task.FromResult(Result.Ok(result));
    }
}
