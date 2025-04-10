namespace Hermes.Core.Dtos.Responses
{
    public record CachePingResultResponseDto
    {
        public bool IsAlive { get; init; }
        public TimeSpan Latency { get; init; }
    }
}