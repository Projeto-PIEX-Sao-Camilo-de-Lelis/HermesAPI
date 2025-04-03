namespace Hermes.Core.Dtos.Responses
{
    public record PostResponseDto
    {
        public Guid Id { get; init; }
        public required string Title { get; init; }
        public required string Content { get; init; }
        public required string ContentPreview { get; init; }
        public required string Author { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public string? Slug { get; init; }
    }
}