using System.Text.Json;
using Hermes.Configs.Cache;
using Hermes.Core.Dtos.Responses;
using Hermes.Core.Interfaces.Cache;
using StackExchange.Redis;

namespace Hermes.Core.Infrastructure.Cache
{
    public class ValkeyCacheProvider : ICacheProvider, IDisposable
    {
        private readonly ConnectionMultiplexer? _valkey;
        private readonly IDatabase? _db;
        private readonly bool _isCacheEnabled;
        private readonly ILogger<ValkeyCacheProvider>? _logger;
        private bool _connectionVerified = false;

        public ValkeyCacheProvider(CacheSettings cacheSettings, ILogger<ValkeyCacheProvider>? logger = null)
        {
            _logger = logger;
            _isCacheEnabled = cacheSettings.IsEnabled;

            if (!_isCacheEnabled)
            {
                _logger?.LogInformation("Cache está desabilitado via configuração");
                return;
            }

            if (string.IsNullOrEmpty(cacheSettings.Endpoint))
            {
                _logger?.LogWarning("Endpoint do cache não configurado");
                return;
            }

            try
            {
                var options = new ConfigurationOptions
                {
                    EndPoints = { { cacheSettings.Endpoint, cacheSettings.Port } },
                    User = cacheSettings.Username,
                    Password = cacheSettings.Password,
                    Ssl = cacheSettings.IsSslEnabled,
                    AbortOnConnectFail = true,
                    ConnectTimeout = 3000,
                    SyncTimeout = 3000,
                    ConnectRetry = 1,
                    CommandMap = CommandMap.Create(new HashSet<string>
                    {
                    }, available: true)
                };

                _valkey = ConnectionMultiplexer.Connect(options);
                _db = _valkey.GetDatabase();

                _logger?.LogInformation("Conexão com cache inicializada");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Erro ao conectar ao provedor de cache");
                _valkey?.Dispose();
                throw;
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            if (!_isCacheEnabled || _valkey is null || _db is null)
            {
                return false;
            }

            try
            {
                await _db.PingAsync();
                _connectionVerified = true;
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Falha no teste de conexão com cache");
                _connectionVerified = false;
                return false;
            }
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            if (!_isCacheEnabled || _db is null || !_connectionVerified)
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
                _logger?.LogWarning(ex, "Erro ao obter chave do cache: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            if (!_isCacheEnabled || _db is null || !_connectionVerified)
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
                _logger?.LogWarning(ex, "Erro ao definir chave no cache: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            if (!_isCacheEnabled || _db is null || !_connectionVerified)
            {
                return;
            }

            try
            {
                await _db.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Erro ao remover chave do cache: {Key}", key);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            if (!_isCacheEnabled || _db is null || !_connectionVerified)
            {
                return false;
            }

            try
            {
                return await _db.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Erro ao verificar existência de chave no cache: {Key}", key);
                return false;
            }
        }

        public async Task ClearAsync(string pattern)
        {
            if (!_isCacheEnabled || _valkey is null || _db is null || !_connectionVerified)
            {
                return;
            }

            try
            {
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
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Erro ao limpar chaves do cache com padrão: {Pattern}", pattern);
            }
        }

        public async Task<CachePingResultResponseDto> PingAsync()
        {
            if (!_isCacheEnabled || _valkey is null || _db is null)
            {
                return new CachePingResultResponseDto
                {
                    IsAlive = false,
                    Latency = TimeSpan.Zero
                };
            }

            try
            {
                var latency = await _db.PingAsync();
                _connectionVerified = true;
                return new CachePingResultResponseDto
                {
                    IsAlive = true,
                    Latency = latency
                };
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Erro ao realizar PING no cache");
                _connectionVerified = false;
                return new CachePingResultResponseDto
                {
                    IsAlive = false,
                    Latency = TimeSpan.Zero
                };
            }
        }

        public void Dispose()
        {
            _valkey?.Dispose();
        }
    }
}