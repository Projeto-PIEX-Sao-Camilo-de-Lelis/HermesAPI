using Hermes.Core.Dtos.Responses;
using Hermes.Core.Interfaces.Cache;

namespace Hermes.Core.Infrastructure.Cache
{
    public class NullCacheProvider : ICacheProvider
    {
        public Task<T?> GetAsync<T>(string key) where T : class
        {
            return Task.FromResult<T?>(null);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key)
        {
            return Task.FromResult(false);
        }

        public Task ClearAsync(string pattern)
        {
            return Task.CompletedTask;
        }

        public Task<CachePingResultResponseDto> PingAsync()
        {
            return Task.FromResult(new CachePingResultResponseDto
            {
                IsAlive = false,
                Latency = TimeSpan.Zero
            });
        }

        public void Dispose()
        {
        }
    }
}