using AutoMapper;
using Hermes.Core.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;
using Hermes.Core.Dtos.Responses;
using Hermes.Core.Dtos.Requests;
using Hermes.Core.Models;
using Microsoft.AspNetCore.Authorization;

namespace Hermes.Controllers
{
    [Route("api/v1/posts")]
    [ApiController]
    [Authorize]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PostController(IPostService postService, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _postService = postService;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PostResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PostResponseDto>> GetAll()
        {
            var existingPosts = await _postService.GetAllPostsAsync();
            var posts = _mapper.Map<IEnumerable<PostResponseDto>>(existingPosts);

            return Ok(posts);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(PostResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PostResponseDto>> GetById(Guid id)
        {
            var existingPost = await _postService.GetPostByIdAsync(id);
            if (existingPost is null)
            {
                return NotFound(new { message = "Nenhum post encontrado com o id especificado." });
            }

            var post = _mapper.Map<PostResponseDto>(existingPost);
            return Ok(post);
        }

        [HttpGet("author/{authorName}")]
        [ProducesResponseType(typeof(IEnumerable<PostResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PostResponseDto>> GetByAuthor(string authorName)
        {
            var existingPosts = await _postService.GetPostByAuthor(authorName);
            if (existingPosts is null)
            {
                return NotFound(new { message = "Nenhum post encontrado com o autor especificado." });
            }
            var posts = _mapper.Map<IEnumerable<PostResponseDto>>(existingPosts);
            return Ok(posts);
        }

        [HttpPost]
        [ProducesResponseType(typeof(PostResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PostResponseDto>> Post([FromBody] PostCreateRequestDto postCreateRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "userId");
            if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out Guid authorId))
            {
                return BadRequest(new { message = "Id do usuário não encontrado ou inválido!" });
            }

            var postToCreate = _mapper.Map<Post>(postCreateRequest);
            postToCreate.AuthorId = authorId;

            var createPost = await _postService.CreatePostAsync(postToCreate);
            var postResponse = _mapper.Map<PostResponseDto>(createPost);

            return CreatedAtAction(nameof(GetById), new { id = postResponse.Id }, postResponse);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(PostResponseDto), StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Post>> Put(Guid id, [FromBody] PostUpdateRequestDto postUpdateRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "userId");
            if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out Guid authorId))
            {
                return BadRequest(new { message = "Id do usuário não encontrado ou inválido!" });
            }

            var existingPost = await _postService.GetPostByIdAsync(id);
            if (existingPost is null)
            {
                return NotFound(new { message = "Nenhum post foi encontrado com o Id informado." });
            }

            var postToUpdate = _mapper.Map(postUpdateRequest, existingPost);
            postToUpdate.AuthorId = authorId;

            await _postService.UpdatePostAsync(id, postToUpdate);
            return NoContent();
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(Guid id)
        {
            var existingPost = await _postService.GetPostByIdAsync(id);
            if (existingPost is null)
            {
                return NotFound(new { message = "Nenhum post foi encontrado com o Id informado." });
            }

            await _postService.DeletePostAsync(id);

            return NoContent();
        }

    }
}
