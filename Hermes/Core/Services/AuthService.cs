using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Hermes.Core.Dtos.Requests;
using Hermes.Core.Interfaces.Service;
using Hermes.Core.Models;
using Microsoft.IdentityModel.Tokens;

namespace Hermes.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;

        public AuthService(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<string?> AuthenticateAsync(UserLoginRequestDto userLoginRequest)
        {
            var user = await _userService.GetUserByEmailAsync(userLoginRequest.Email);

            if (user is null || !user.CheckPassword(userLoginRequest.Password))
            {
                return null;
            }

            return AuthorizeUser(user);
        }

        private static string AuthorizeUser(User user)
        {
            return GenerateJwtToken(user);
        }

        private static string GenerateJwtToken(User user)
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
                Expires = DateTime.UtcNow.AddMinutes(
                    int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES"), out int expiration) ? expiration : 30),
                Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
                Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
                SigningCredentials = GetCredentials(),
            };
        }
    }
}