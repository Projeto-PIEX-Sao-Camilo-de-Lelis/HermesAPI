using System.Security.Cryptography;
using System.Text;
using Slugify;

namespace Hermes.Helpers
{
    public static class ShortHandSlugGenerator
    {
        private static readonly SlugHelper _slugHelper = new();

        public static string GenerateSlug(string text)
        {
            return _slugHelper.GenerateSlug(text);
        }

        public static string GenerateUniqueSlug(string text, bool exists)
        {
            if (exists)
            {
                string hash = GenerateHash(text);
                hash += GenerateRandomNumber(text.Length);

                return _slugHelper.GenerateSlug($"{text}-{hash}");
            }
            else
            {
                return _slugHelper.GenerateSlug(text);
            }
        }

        private static string GenerateHash(string input)
        {
            input += DateTime.UtcNow.ToString();
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            StringBuilder builder = new();

            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString()[..5];
        }

        private static int GenerateRandomNumber(int inputLength)
        {
            Random random = new();
            int rnd = random.Next(0, 999);
            return rnd * inputLength;
        }
    }
}