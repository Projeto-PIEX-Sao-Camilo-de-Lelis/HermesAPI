namespace Hermes.Configs.JsonSerializer
{
    public static class JsonSerializerConfiguration
    {
        public static IServiceCollection ConfigureJsonSerializer(this IServiceCollection services)
        {
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });

            return services;
        }
    }
}
