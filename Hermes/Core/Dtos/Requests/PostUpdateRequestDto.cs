namespace Hermes.Core.Dtos.Requests
{
    public record PostUpdateRequestDto
    {
        public string? Title { get; init; } = string.Empty;
        public string? Content { get; init; } = string.Empty;
    }
}