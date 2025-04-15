using Hermes.Core.Dtos.Requests;
using Hermes.Core.Dtos.Responses;
using Hermes.Core.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;

namespace Hermes.Controllers
{
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authenticationService)
        {
            _authService = authenticationService;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Login([FromBody] UserLoginRequestDto userLoginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var authResult = await _authService.AuthenticateAsync(userLoginRequest);
            if (authResult is null)
            {
                return Unauthorized(new
                {
                    Sucess = false,
                    Message = "E-mail ou senha incorretos. Por favor, verifique suas credenciais e tente novamente.",
                });
            }

            return Ok(new
            {
                Token = authResult,
            });
        }
    }
}
