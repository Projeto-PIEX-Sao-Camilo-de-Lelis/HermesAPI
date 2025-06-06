﻿using Hermes.Core.Dtos.Responses;

namespace Hermes.Core.Interfaces.Cache
{
    public interface ICacheProvider
    {
        Task<T?> GetAsync<T>(string key) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
        Task ClearAsync(string pattern);
        Task<CachePingResultResponseDto> PingAsync();
    }
}