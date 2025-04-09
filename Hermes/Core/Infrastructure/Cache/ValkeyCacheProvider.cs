using System.Text.Json;
using Hermes.Core.Interfaces.Cache;
using StackExchange.Redis;

namespace Hermes.Core.Infrastructure.Cache
{
    public class ValkeyCacheProvider : ICacheProvider
    {
        private readonly ConnectionMultiplexer? _valkey;
        private readonly IDatabase? _db;
        private readonly bool _isCacheEnabled;

        public ValkeyCacheProvider(
            bool isCacheEnabled,
            string endpoint,
            int port,
            bool isSslEnabled,
            bool isAbortOnConnectFailEnabled,
            string username,
            string password)
        {
            _isCacheEnabled = isCacheEnabled;

            if (_isCacheEnabled)
            {
                var options = new ConfigurationOptions
                {
                    EndPoints = { { endpoint, port } },
                    User = username,
                    Password = password,
                    Ssl = isSslEnabled,
                    AbortOnConnectFail = isAbortOnConnectFailEnabled,
                };

                _valkey = ConnectionMultiplexer.Connect(options);
                _db = _valkey.GetDatabase();
            }
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            if (!_isCacheEnabled || _db is null)
            {
                return null;
            }

            var value = await _db.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(value!);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            if (!_isCacheEnabled || _db is null)
            {
                return;
            }

            var serializedValue = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, serializedValue, expiration);
        }

        public async Task RemoveAsync(string key)
        {
            if (!_isCacheEnabled || _db is null)
            {
                return;
            }

            await _db.KeyDeleteAsync(key);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            if (!_isCacheEnabled || _db is null)
            {
                return false;
            }

            return await _db.KeyExistsAsync(key);
        }

        public async Task ClearAsync(string pattern)
        {
            if (!_isCacheEnabled || _valkey is null || _db is null)
            {
                return;
            }

            var endpoints = _valkey.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _valkey.GetServer(endpoint);
                var keys = server.Keys(pattern: pattern);

                foreach (var key in keys)
                {
                    await _db.KeyDeleteAsync(key);
                }
            }
        }
    }
}