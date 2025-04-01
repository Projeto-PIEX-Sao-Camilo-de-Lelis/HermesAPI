using AutoMapper;
using Hermes.Core.Dtos.Requests;
using Hermes.Core.Dtos.Responses;
using Hermes.Core.Interfaces.Service;
using Hermes.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hermes.Controllers
{
    [Route("api/v1/users")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UsersController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<UserResponseDto>> GetAll()
        {
            var existingUser = await _userService.GetAllUsersAsync();
            var users = _mapper.Map<IEnumerable<UserResponseDto>>(existingUser);

            return Ok(users);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserResponseDto>> GetById(Guid id)
        {
            var existingUser = await _userService.GetUserByIdAsync(id);
            if (existingUser is null)
            {
                return NotFound();
            }
            var user = _mapper.Map<UserResponseDto>(existingUser);

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

            var userToCreate = _mapper.Map<User>(userCreateRequest);
            var createdUser = await _userService.CreateUserAsync(userToCreate);

            var userResponse = _mapper.Map<UserResponseDto>(createdUser);

            return CreatedAtAction(nameof(GetById), new { userResponse.Id }, userResponse);
        }

        [HttpPut]
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

            var userToUpdate = _mapper.Map(userUpdateRequest, existingUser);
 
            await _userService.UpdateUserAsync(userToUpdate);
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
