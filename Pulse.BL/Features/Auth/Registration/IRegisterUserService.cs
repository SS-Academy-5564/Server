
namespace Pulse.BL.Features.Auth.Registration;

public interface IRegisterUserService
{
    Task Register(RegisterUserRequest request);
}
