using Slugify;

namespace Hermes.Helpers
{
    public static class ShortHandSlugGenerator
    {
        public static string GenerateSlug(string text)
        {
            SlugHelper slugHelper = new();

            DateOnly date = DateOnly.FromDateTime(DateTime.UtcNow);
            _ = date.ToString("dd-MM-yyyy");
            string slug = slugHelper.GenerateSlug($"{date}-{text}");

            return slug;
        }
    }
}