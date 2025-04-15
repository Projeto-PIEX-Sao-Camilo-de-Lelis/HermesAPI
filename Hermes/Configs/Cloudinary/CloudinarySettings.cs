namespace Hermes.Configs.Cloudinary
{
    public class CloudinarySettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public string CloudName { get; set; } = string.Empty;
        //public string CloudinaryUrl { get; set; } = string.Empty;
        public string UploadPreset { get; set; } = string.Empty;
        public string Folder { get; set; } = string.Empty;
    }
}