namespace Hermes.Core.Dtos.Requests
{
    public record BlogPostCreateRequestDto
    {
        public required string Title { get; init; }
        public required string Content { get; init; }
    }
}