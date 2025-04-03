namespace Hermes.Helpers
{
    public static class ContentPreviewGenerator
    {
        public static string GeneratePreview(string content, int maxLength = 150)
        {
            if (string.IsNullOrEmpty(content))
            {
                return string.Empty;
            }

            var textOnlyContent = System.Text.RegularExpressions.Regex.Replace(content, "<.*?>", string.Empty);

            if (textOnlyContent.Length <= maxLength)
            {
                return textOnlyContent;
            }

            return string.Concat(textOnlyContent.AsSpan(0, maxLength), "...");
        }
    }
}
