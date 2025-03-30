using Hermes.Core.Enums;

namespace Hermes.Core.Dtos.Requests
{
    public record UserAuthRequestDto
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public string Email { get; init; } = string.Empty;

        public string Password { get; init; } = string.Empty;

        public Role UserRole { get; init; } = Role.User;
    }
}