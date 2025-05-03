using Hermes.Core.Interfaces.Service;
using Hermes.Core.Services;

namespace Hermes.Configs.Cloudinary
{
    public static class CloudinaryConfiguration
    {
        public static IServiceCollection AddCloudinaryConfiguration(this IServiceCollection services)
        {
            var cloudinarySettings = new CloudinarySettings
            {
                ApiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY") ?? string.Empty,
                ApiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET") ?? string.Empty,
                CloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME") ?? string.Empty,
                UploadPreset = Environment.GetEnvironmentVariable("CLOUDINARY_UPLOAD_PRESET") ?? string.Empty,
                Folder = Environment.GetEnvironmentVariable("CLOUDINARY_FOLDER") ?? string.Empty
            };

            services.AddSingleton(cloudinarySettings);
            services.AddScoped<IImageService, ImageService>();

            return services;
        }
    }
}