namespace Hermes.Core.Dtos.Requests
{
    public record UserLoginRequestDto
    {
        public required string Email { get; init; }
        public required string Password { get; init; }
    }
}