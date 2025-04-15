using Hermes.Core.Dtos.Responses;
using Hermes.Core.Interfaces.Cache;

namespace Hermes.Core.Infrastructure.Cache
{
    public class NullCacheProvider : ICacheProvider
    {
        public Task<T?> GetAsync<T>(string key) where T : class => Task.FromResult<T?>(null);
        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class => Task.CompletedTask;
        public Task RemoveAsync(string key) => Task.CompletedTask;
        public Task<bool> ExistsAsync(string key) => Task.FromResult(false);
        public Task ClearAsync(string pattern) => Task.CompletedTask;
        public Task<CachePingResultResponseDto> PingAsync() => Task.FromResult(new CachePingResultResponseDto { IsAlive = false, Latency = TimeSpan.Zero });
    }
}