namespace Hermes.Core.Interfaces.Service
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile imageFile);
        Task<string> UploadImageAsync(byte[] imageBytes, string fileName);
        Task<bool> DeleteImageAsync(string publicId);
        string GetPublicIdFromUrl(string imageUrl);
    }
}