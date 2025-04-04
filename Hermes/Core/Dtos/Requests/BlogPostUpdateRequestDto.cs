namespace Hermes.Core.Dtos.Requests
{
    public record BlogPostUpdateRequestDto
    {
        public required string Title { get; init; } = string.Empty;
        public required string Content { get; init; } = string.Empty;
    }
}