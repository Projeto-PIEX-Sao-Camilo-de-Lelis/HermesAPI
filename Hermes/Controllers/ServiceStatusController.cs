using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hermes.Controllers
{
    [Route("api/v1/healthz")]
    [ApiController]
    public class ServiceStatusController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            var healthCheck = new
            {
                status = "Serviço executando..."
            };

            return Ok(healthCheck);
        }
    }
}
