namespace Hermes.Core.Dtos.Responses
{
    public record AuthResponseDto
    {
        public bool Sucess { get; init; }
        public string? Message {  get; init; }
        public string? Token { get; init; }
    }
}