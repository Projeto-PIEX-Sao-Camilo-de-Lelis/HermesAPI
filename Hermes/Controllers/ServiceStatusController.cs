using Hermes.Core.Interfaces.Cache;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hermes.Controllers
{
    [Route("api/v1/healthz")]
    [ApiController]
    public class ServiceStatusController : ControllerBase
    {
        private readonly ICacheProvider _valkeyCacheProvider;

        public ServiceStatusController(ICacheProvider valkeyCacheProvider)
        {
            _valkeyCacheProvider = valkeyCacheProvider;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            string cacheEnabledStatus = Environment.GetEnvironmentVariable("CACHE_ENABLED") ?? "false";
            var cachePingResult = await _valkeyCacheProvider.PingAsync();


            var healthCheck = new
            {
                serviceStatus = "Up",
                cacheEnabledStatus,
                cacheStatus = cachePingResult.IsAlive ? "Up" : "Down",
                cacheLatency = cachePingResult.Latency.TotalMilliseconds + " ms"
            };

            return Ok(healthCheck);
        }
    }
}