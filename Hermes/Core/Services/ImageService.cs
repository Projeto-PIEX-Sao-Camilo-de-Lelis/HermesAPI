using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Hermes.Configs.Cloudinary;
using Hermes.Configs.Constants;
using Hermes.Core.Interfaces.Service;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Size = SixLabors.ImageSharp.Size;

namespace Hermes.Core.Services
{
    public class ImageService : IImageService
    {
        private readonly Cloudinary _cloudinary;
        private readonly CloudinarySettings _cloudinarySettings;
        private readonly ILogger<ImageService> _logger;

        public ImageService(CloudinarySettings settings, ILogger<ImageService> logger)
        {
            _cloudinarySettings = settings;

            var account = new Account(
                settings.CloudName,
                settings.ApiKey,
                settings.ApiSecret);

            _cloudinary = new Cloudinary(account);
            _logger = logger;
        }

        public async Task<string> UploadImageAsync(IFormFile imageFile)
        {
            try
            {
                using var stream = imageFile.OpenReadStream();
                using var optimizedImage = await OptimizeImageAsync(stream);
                using var optimizedStream = new MemoryStream();
                await optimizedImage.SaveAsJpegAsync(optimizedStream);
                optimizedStream.Position = 0;

                var uploadParams = new ImageUploadParams
                {
                    Overwrite = true,
                    File = new FileDescription(imageFile.FileName, optimizedStream),
                    UseFilename = true,
                    UniqueFilename = true,
                    Folder = _cloudinarySettings.Folder,
                    UploadPreset = _cloudinarySettings.UploadPreset,
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error is not null)
                {
                    _logger.LogError("Ocorreu um erro ao tentar fazer o upload da imagem: {ErrorMessage}", uploadResult.Error.Message);
                    throw new InvalidOperationException($"Falha ao fazer upload da imagem: {uploadResult.Error.Message}");
                }

                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao tentar fazer upload da imagem para o Cloudinary");
                throw new InvalidOperationException("Falha ao processar o upload da imagem", ex);
            }
        }

        public async Task<string> UploadImageAsync(byte[] imageBytes, string fileName)
        {
            try
            {
                using var stream = new MemoryStream(imageBytes);
                using var optimizedImage = await OptimizeImageAsync(stream);
                using var optimizedStream = new MemoryStream();
                await optimizedImage.SaveAsJpegAsync(optimizedStream);

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(fileName, optimizedStream),
                    UseFilename = true,
                    UniqueFilename = true,
                    Folder = _cloudinarySettings.Folder,
                    UploadPreset = _cloudinarySettings.UploadPreset,
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error is not null)
                {
                    _logger.LogError("Ocorreu um erro ao tentar fazer o upload da imagem: {ErrorMessage}", uploadResult.Error.Message);
                    throw new InvalidOperationException($"Falha ao fazer upload da imagem: {uploadResult.Error.Message}");
                }

                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao tentar fazer upload da imagem para o Cloudinary");
                throw new InvalidOperationException("Falha ao processar o upload da imagem", ex);
            }
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            try
            {
                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);

                if (result.Error is not null)
                {
                    _logger.LogError("Ocorreu um erro ao tentar deletar a imagem: {ErrorMessage}", result.Error.Message);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao tentar deletar a imagem do Cloudinary");
                return false;
            }
        }

        public string GetPublicIdFromUrl(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                {
                    return string.Empty;
                }

                var uri = new Uri(imageUrl);
                var path = uri.AbsolutePath;

                var publicIdWithExtension = Path.GetFileName(path);
                var publicId = Path.GetFileNameWithoutExtension(publicIdWithExtension);

                if (path.Contains("/" + _cloudinarySettings.Folder + "/"))
                {
                    var parts = path.Split('/');
                    for (int i = 1; i < parts.Length - 1; i++)
                    {
                        if (parts[i] == _cloudinarySettings.Folder)
                        {
                            return $"{parts[i]}/{publicId}";
                        }
                    }
                }

                return publicId;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static async Task<Image> OptimizeImageAsync(Stream stream)
        {
            var image = await Image.LoadAsync(stream);

            if (image.Width > ImageConstants.MaxWidthInPixels)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(ImageConstants.MaxWidthInPixels),
                    Mode = ResizeMode.Max
                }));
            }

            return image;
        }
    }
}