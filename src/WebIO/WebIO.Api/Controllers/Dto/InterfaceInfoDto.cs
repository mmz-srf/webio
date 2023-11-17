namespace WebIO.Api.Controllers.Dto;

public record InterfaceInfoDto
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Template { get; init; }

    public StreamCardinalityDto Streams { get; init; } = new();
}