namespace Hermes.Core.Dtos.Requests
{
    public record PostCreateRequestDto
    {
        public required string Title { get; init; }
        public required string Content { get; init; }
    }
}