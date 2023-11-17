namespace WebIO.Api.Controllers.Dto;

public record PropertyChangedEventDto
{
    public required string Property { get; init; }
    public required string NewValue { get; init; }
    public required string Entity { get; init; }
    public required string Device { get; init; }
    public required string EntityType { get; init; }
}