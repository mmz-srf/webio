namespace WebIO.Api.Controllers.Dto;

public record InterfaceDto: EntityBaseDto
{
    public required string InterfaceTemplate { get; init; }
}