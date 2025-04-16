using Hermes.Configs.Constants;
using Hermes.Core.Dtos.Responses;
using Hermes.Core.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hermes.Controllers
{
    [Route("api/v1/images")]
    [ApiController]
    [Authorize]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly ILogger<ImageController> _logger;

        public ImageController(IImageService imageService, ILogger<ImageController> logger)
        {
            _imageService = imageService;
            _logger = logger;
        }

        [HttpPost("upload")]
        [ProducesResponseType(typeof(ImageUploadResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file is null || file.Length == 0)
            {
                return BadRequest(new { message = "Nenhuma imagem foi enviada." });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtensions = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtensions))
            {
                return BadRequest(new { message = "Tipo de arquivo não permitido. Por favor, envie apenas imagens nos formatos JPG, PNG ou GIF" });
            }

            if (file.Length > ImageConstants.MaxImageSizeInMB * 1024 * 1024)
            {
                return BadRequest(new { message = $"O tamanho da imagem excede o limite permitido de {ImageConstants.MaxImageSizeInMB}MB." });
            }

            try
            {
                var imageUrl = await _imageService.UploadImageAsync(file);
                return Ok(new ImageUploadResponseDto
                {
                    Url = imageUrl,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao tentar fazer o upload da imagem");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Ocorreu um erro ao tentar processar a imagem." });
            }
        }

        [HttpDelete("delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteImage([FromQuery] string imageUrl = "", [FromQuery] string publicId = "")
        {
            if (string.IsNullOrWhiteSpace(imageUrl) && string.IsNullOrWhiteSpace(publicId))
            {
                return BadRequest(new { message = "É necessário fornecer uma URL de imagem ou um Id válido para deletar a imagem." });
            }

            try
            {
                string idToDelete = publicId;

                if (string.IsNullOrWhiteSpace(publicId) && !string.IsNullOrWhiteSpace(imageUrl))
                {
                    idToDelete = _imageService.GetPublicIdFromUrl(imageUrl);
                    if (string.IsNullOrEmpty(idToDelete))
                    {
                        return BadRequest(new { message = "Não foi possível extrair o Id´da URL fornecida." });
                    }
                }

                var result = await _imageService.DeleteImageAsync(idToDelete);
                if (result)
                {
                    return Ok(new
                    {
                        message = "Imagem deletada com sucesso.",
                        success = true
                    });
                }

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Ocorreu um erro ao tentar deletar a imagem." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao tentar deletar a imagem com ID: {PublicId}", publicId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Ocorreu um erro ao tentar deletar a imagem." });
            }
        }
    }
}
