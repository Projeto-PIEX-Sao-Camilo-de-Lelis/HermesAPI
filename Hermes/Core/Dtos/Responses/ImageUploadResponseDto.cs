namespace Hermes.Core.Dtos.Responses
{
    public record ImageUploadResponseDto
    {
        public string Url { get; init; } = string.Empty;
        public bool Success { get; init; }
    }
}