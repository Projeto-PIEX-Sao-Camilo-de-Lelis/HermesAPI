namespace Hermes.Configs.Cache
{
    public class CacheSettings
    {
        public bool IsEnabled { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Expiration { get; set; }
        public string Provider { get; set; } = "Valkey";
    }
}