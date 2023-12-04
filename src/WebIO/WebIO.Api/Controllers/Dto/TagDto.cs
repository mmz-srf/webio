namespace WebIO.Api.Controllers.Dto;

public record TagDto
{
    public required string Name { get; init; }
    public required string StreamType { get; init; }
}