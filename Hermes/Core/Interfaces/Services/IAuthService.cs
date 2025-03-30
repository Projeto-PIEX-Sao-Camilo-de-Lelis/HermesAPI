using Hermes.Core.Dtos.Requests;
using Hermes.Core.Dtos.Responses;

namespace Hermes.Core.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> AuthenticateAsync(UserLoginRequestDto userLoginRequest);
        void Logout();
    }
}