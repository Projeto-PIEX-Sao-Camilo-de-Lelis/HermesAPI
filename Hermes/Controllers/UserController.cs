using Hermes.Core.Dtos.Requests;
using Hermes.Core.Dtos.Responses;
using Hermes.Core.Extensions;
using Hermes.Core.Interfaces.Service;
using Hermes.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hermes.Controllers
{
    [Route("api/v1/users")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<UserResponseDto>> GetAll()
        {
            var existingUsers = await _userService.GetAllUsersAsync();
            var users = UserMapper.ToResponseDto(existingUsers);

            return Ok(users);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserResponseDto>> GetById(Guid id)
        {
            var existingUser = await _userService.GetUserByIdAsync(id);
            if (existingUser is null)
            {
                return NotFound(new { message = "Nenhum usuário encontrado com o id especificado." });
            }
            var user = UserMapper.ToResponseDto(existingUser);

            return Ok(user);
        }

        [HttpPost]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [AllowAnonymous]
        public async Task<ActionResult<UserResponseDto>> Post([FromBody] UserCreateRequestDto userCreateRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var userToCreate = UserMapper.ToEntity(userCreateRequest);
            var createdUser = await _userService.CreateUserAsync(userToCreate);

            var userResponse = UserMapper.ToResponseDto(createdUser);

            return CreatedAtAction(nameof(GetById), new { userResponse.Id }, userResponse);
        }

        [HttpPut("{email}")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserResponseDto>> Put(string email, [FromBody] UserUpdateRequestDto userUpdateRequest )
        {
            if (email != userUpdateRequest.Email)
            {
                return BadRequest("E-mail incorreto ou inexistente.");
            }
   
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUser = await _userService.GetUserByEmailAsync(email);
            if (existingUser is null)
            {
                return NotFound();
            }

            UserMapper.UpdateEntity(existingUser, userUpdateRequest);
            await _userService.UpdateUserAsync(existingUser.Id, existingUser);

            return NoContent();
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(string email)
        {
            var existingUser = await _userService.GetUserByEmailAsync(email);
            if (existingUser is null)
            {
                return NotFound();
            }

            await _userService.DeleteUserAsync(existingUser.Id);
            return NoContent();
        }
    }
}
