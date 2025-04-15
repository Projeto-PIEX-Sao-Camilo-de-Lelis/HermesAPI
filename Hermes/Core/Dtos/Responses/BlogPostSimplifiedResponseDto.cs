namespace Hermes.Core.Dtos.Responses
{
    public record BlogPostSimplifiedResponseDto
    {
        public Guid Id { get; init; }
        public string? Slug { get; init; }
        public required string Title { get; init; }
        public required string ContentPreview { get; init; }
        public required string Author { get; init; }
        public required DateOnly PublishedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }
}