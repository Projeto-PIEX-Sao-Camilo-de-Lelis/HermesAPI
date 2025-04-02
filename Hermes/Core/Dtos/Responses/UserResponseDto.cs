namespace Hermes.Core.Dtos.Responses
{
    public record UserResponseDto
    {
        public Guid Id { get; init; }
        public required string Name { get; init; }
        public required string Email { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public bool IsActive { get; init; }
    }
}
