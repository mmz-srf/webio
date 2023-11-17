namespace WebIO.Api.Controllers.Dto;

public record DeviceDto : EntityBaseDto
{
    public required string DeviceType { get; init; }
}