using FluentResults;
using Pulse.BL.Common.Handlers;

namespace Pulse.BL.Features.Users.Me;

public interface ICurrentUserQuery : IAsyncQuery<Result<UserProfileResult>>
{
}
