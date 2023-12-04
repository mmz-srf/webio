namespace WebIO.Api.Controllers.Dto;

public record PropertiesChangedSummaryDto: ChangeEventDto
{
    public List<PropertyChangedEventDto> ChangedEvents { get; init; }  = new();
}