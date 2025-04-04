using Hermes.Core.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;
using Hermes.Core.Dtos.Responses;
using Hermes.Core.Dtos.Requests;
using Hermes.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Hermes.Core.Extensions;

namespace Hermes.Controllers
{
    [Route("api/v1/posts")]
    [ApiController]
    [Authorize]
    public class BlogPostController : ControllerBase
    {
        private readonly IBlogPostService _postService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BlogPostController(IBlogPostService postService, IHttpContextAccessor httpContextAccessor)
        {
            _postService = postService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResponseDto<BlogPostResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResponseDto<BlogPostResponseDto>>> GetPaged([FromQuery] PaginationRequestDto pagination)
        {
            var (posts, totalCount) = await _postService.GetPagedPostsAsync(pagination.PageNumber, pagination.PageSize);
            var postsDto = BlogPostMapper.ToResponseDto(posts);

            var pagedResponse = new PagedResponseDto<BlogPostResponseDto>(
                postsDto,
                pagination.PageNumber,
                pagination.PageSize,
                totalCount
            );

            return Ok(pagedResponse);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BlogPostResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BlogPostResponseDto>> GetById(Guid id)
        {
            var existingPost = await _postService.GetPostByIdAsync(id);
            if (existingPost is null)
            {
                return NotFound(new { message = "Nenhum post encontrado com o id especificado." });
            }

            var post = BlogPostMapper.ToResponseDto(existingPost);
            return Ok(post);
        }

        [HttpGet("slug/{slug}")]
        [ProducesResponseType(typeof(BlogPostResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BlogPostResponseDto>> GetBySlug(string slug)
        {
            var existingPost = await _postService.GetPostBySlugAsync(slug);
            if (existingPost is null)
            {
                return NotFound(new { message = "Nenhum post encontrado com a slug especificada." });
            }
            var posts = BlogPostMapper.ToResponseDto(existingPost);

            return Ok(posts);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BlogPostResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BlogPostResponseDto>> Post([FromBody] BlogPostCreateRequestDto postCreateRequest)
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

            var postToCreate = postCreateRequest.ToEntity();
            postToCreate.AuthorId = authorId;

            var createPost = await _postService.CreatePostAsync(postToCreate);
            var postResponse = BlogPostMapper.ToResponseDto(createPost);

            return CreatedAtAction(nameof(GetById), new { id = postResponse.Id }, postResponse);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(BlogPostResponseDto), StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BlogPost>> Put(Guid id, [FromBody] BlogPostUpdateRequestDto postUpdateRequest)
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

            BlogPostMapper.UpdateEntity(existingPost, postUpdateRequest);
            existingPost.AuthorId = authorId;

            await _postService.UpdatePostAsync(id, existingPost);
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
