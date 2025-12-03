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
            bool isCacheEnabled = bool.TryParse(cacheEnabledStatus, out bool enabled) && enabled;

            var healthCheck = new
            {
                serviceStatus = "Up",
                cacheEnabledStatus,
                cacheStatus = await GetCacheStatusAsync(isCacheEnabled),
                cacheLatency = await GetCacheLatencyAsync(isCacheEnabled),
            };

            return Ok(healthCheck);
        }

        private async Task<string> GetCacheStatusAsync(bool isCacheEnabled)
        {
            if (!isCacheEnabled)
            {
                return "Disabled";
            }

            try
            {
                var cachePingResult = await _valkeyCacheProvider.PingAsync();
                return cachePingResult.IsAlive ? "Up" : "Down";
            }
            catch
            {
                return "Down";
            }
        }

        private async Task<string> GetCacheLatencyAsync(bool isCacheEnabled)
        {
            if (!isCacheEnabled)
            {
                return "NA";
            }

            try
            {
                var cachePingResult = await _valkeyCacheProvider.PingAsync();
                return cachePingResult.Latency.TotalMilliseconds + " ms";
            }
            catch
            {
                return "NA";
            }
        }
    }
}