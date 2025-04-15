using Hermes.Core.Dtos.Requests;
using Hermes.Core.Dtos.Responses;

namespace Hermes.Core.Interfaces.Service
{
    public interface IAuthService
    {
        Task<string?> AuthenticateAsync(UserLoginRequestDto userLoginRequest);
    }
}