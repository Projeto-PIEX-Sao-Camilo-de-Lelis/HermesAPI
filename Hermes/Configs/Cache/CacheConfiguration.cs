using Hermes.Core.Infrastructure.Cache;
using Hermes.Core.Interfaces.Cache;
using Hermes.Core.Services.Cache;

namespace Hermes.Configs.Cache
{
    public static class CacheConfiguration
    {
        public static IServiceCollection AddCacheConfiguration(this IServiceCollection services)
        {
            var cacheSettings = new CacheSettings
            {
                IsEnabled = bool.TryParse(Environment.GetEnvironmentVariable("CACHE_ENABLED"), out bool enabled) && enabled,
                Endpoint = Environment.GetEnvironmentVariable("CACHE_ENDPOINT") ?? string.Empty,
                Password = Environment.GetEnvironmentVariable("CACHE_PASSWORD") ?? string.Empty,
                Username = Environment.GetEnvironmentVariable("CACHE_USERNAME") ?? "default",
                Port = int.TryParse(Environment.GetEnvironmentVariable("CACHE_PORT"), out int port) ? port : 6379,
                IsSslEnabled = bool.TryParse(Environment.GetEnvironmentVariable("CACHE_SSL_ENABLED"), out bool sslEnabled) && sslEnabled,
                IsAbortOnConnectFailEnabled = bool.TryParse(Environment.GetEnvironmentVariable("CACHE_ABORT_ON_CONNECT_FAIL_ENABLED"), out bool abortOnConnectFail) && abortOnConnectFail,
                Expiration = int.TryParse(Environment.GetEnvironmentVariable("CACHE_EXPIRATION_MINUTES"), out int expiration) ? expiration : 15,
                Provider = Environment.GetEnvironmentVariable("CACHE_PROVIDER") ?? "Valkey"
            };

            services.AddSingleton(cacheSettings);

            if (cacheSettings.IsEnabled && !string.IsNullOrEmpty(cacheSettings.Endpoint))
            {
                services.AddSingleton<ICacheProvider>(provider =>
                    new ValkeyCacheProvider(cacheSettings));
            }
            else
            {
                services.AddSingleton<ICacheProvider, NullCacheProvider>();
            }

            services.AddScoped<IBlogPostCacheService, BlogPostCacheService>();

            return services;
        }
    }
}
