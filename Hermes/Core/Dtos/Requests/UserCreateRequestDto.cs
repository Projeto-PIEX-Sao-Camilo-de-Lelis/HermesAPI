using Hermes.Core.Enums;

namespace Hermes.Core.Dtos.Requests
{
    public record UserCreateRequestDto
    {
        public string Name { get; init; }
        public string Email { get; init; }
        public string Password { get; init; }
        public Role Role { get; init; }
    }
}