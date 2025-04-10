using System.Text.Json;
using Hermes.Core.Dtos.Responses;
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
                try
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
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao conectar ao Redis: {ex.Message}");
                    _isCacheEnabled = false;
                }
            }
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            if (!_isCacheEnabled || _db is null)
            {
                return null;
            }

            try
            {
                var value = await _db.StringGetAsync(key);
                if (value.IsNullOrEmpty)
                {
                    return null;
                }

                return JsonSerializer.Deserialize<T>(value!);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter chave do cache: {ex.Message}");
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            if (!_isCacheEnabled || _db is null)
            {
                return;
            }

            try
            {
                var serializedValue = JsonSerializer.Serialize(value);
                await _db.StringSetAsync(key, serializedValue, expiration);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao definir chave no cache: {ex.Message}");
            }
        }

        public async Task RemoveAsync(string key)
        {
            if (!_isCacheEnabled || _db is null)
            {
                return;
            }

            try
            {
                await _db.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao remover chave do cache: {ex.Message}");
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            if (!_isCacheEnabled || _db is null)
            {
                return false;
            }

            try
            {
                return await _db.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao verificar existência de chave no cache: {ex.Message}");
                return false;
            }
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

        public async Task<CachePingResultResponseDto> PingAsync()
        {
            if (!_isCacheEnabled || _valkey is null)
            {
                return new CachePingResultResponseDto
                {
                    IsAlive = false,
                    Latency = TimeSpan.Zero
                };
            }

            try
            {
                var db = _valkey.GetDatabase();
                var latency = await db.PingAsync();
                return new CachePingResultResponseDto
                {
                    IsAlive = true,
                    Latency = latency
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao realizar PING no cache: {ex.Message}");
                return new CachePingResultResponseDto
                {
                    IsAlive = false,
                    Latency = TimeSpan.Zero
                };
            }
        }
    }
}