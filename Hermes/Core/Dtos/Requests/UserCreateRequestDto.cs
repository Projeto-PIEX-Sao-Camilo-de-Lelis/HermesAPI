using Hermes.Core.Enums;

namespace Hermes.Core.Dtos.Requests
{
    public record UserCreateRequestDto
    {
        public required string Name { get; init; }
        public required string Email { get; init; }
        public required string Password { get; init; }
        public Role Role { get; init; }
    }
}