namespace WebIO.Api.Controllers.Dto;

public record DeviceAddedEventDto: ChangeEventDto
{
    public required string Name { get; init; }
    public required string DeviceType { get; init; }
    public List<InterfaceTemplateSelectionDto> Interfaces { get; init; } = new();
    public bool UseSt20227 { get; init; }
}