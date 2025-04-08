using Hermes.Core.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hermes.Controllers
{
    [Route("api/v1/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IBlogPostService _blogPostService;

        public AdminController(IBlogPostService blogPostService)
        {
            _blogPostService = blogPostService;
        }

        [HttpPost("synchronize-cache")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SynchronizeBlogPostCache()
        {
            await _blogPostService.SynchronizeBlogPostsCacheAsync();
            return Ok(new { message = "Cache sincronizado com sucesso!" });
        }
    }
}