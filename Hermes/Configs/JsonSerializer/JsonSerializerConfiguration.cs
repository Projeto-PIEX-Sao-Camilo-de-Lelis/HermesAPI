namespace Hermes.Configs.JsonSerializer
{
    public static class JsonSerializerConfiguration
    {
        public static IServiceCollection AddJsonSerializerConfiguration(this IServiceCollection services)
        {
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                options.JsonSerializerOptions.Converters.Add(new Converters.DateTimeJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new Converters.DateOnlyJsonConverter());
            });

            return services;
        }
    }
}