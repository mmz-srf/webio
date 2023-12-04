namespace WebIO.Api.Controllers.Dto;

public record DeviceUpdatedEventDto : ChangeEventDto
{
    public Guid DeviceId { get; init; }
    public List<InterfaceTemplateSelectionDto> Interfaces { get; init; } = new();
}