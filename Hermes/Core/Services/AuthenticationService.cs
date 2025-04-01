using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Hermes.Configs.Constants;
using Hermes.Core.Dtos.Requests;
using Hermes.Core.Dtos.Responses;
using Hermes.Core.Interfaces.Service;
using Hermes.Core.Models;
using Microsoft.IdentityModel.Tokens;

namespace Hermes.Core.Services
{
    public class AuthenticationService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticationService(IUserService userService, IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuthResponseDto?> AuthenticateAsync(UserLoginRequestDto userLoginRequest)
        {
            var user = await _userService.GetUserByEmailAsync(userLoginRequest.Email);
            if (user is null || !user.CheckPassword(userLoginRequest.Password))
            {
                return null;
            }

            return AuthorizeUser(user);
        }

        public void Logout()
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Path = "/",
                    Expires = DateTime.UtcNow.AddDays(-1)
                };

                _httpContextAccessor.HttpContext.Response.Cookies.Append(AuthConstants.AuthTokenCookieName, "", cookieOptions);
            }
        }

        private AuthResponseDto? AuthorizeUser(User user)
        {
            var token = GenerateJwtToken(user);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTime.UtcNow.AddHours(1),
            };

            _httpContextAccessor.HttpContext?.Response.Cookies.Append(AuthConstants.AuthTokenCookieName, token, cookieOptions);

            return new AuthResponseDto
            {
                Sucess = true,
                Message = "Autenticado com sucesso!",
            };
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(GetTokenDescriptor(user));
            return tokenHandler.WriteToken(token);
        }

        private static SigningCredentials GetCredentials()
        {
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_PRIVATE_KEY")!));
            return new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
        }

        private static ClaimsIdentity GenerateClaims(User user)
        {
            var ci = new ClaimsIdentity();
            ci.AddClaim(new Claim("userId", user.Id.ToString()));
            ci.AddClaim(new Claim("username", user.Name));
            ci.AddClaim(new Claim(ClaimTypes.Name, user.Email));
            ci.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()));

            return ci;
        }

        private static SecurityTokenDescriptor GetTokenDescriptor(User user)
        {
            return new SecurityTokenDescriptor
            {
                Subject = GenerateClaims(user),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
                Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
                SigningCredentials = GetCredentials(),
            };
        }
    }
}
