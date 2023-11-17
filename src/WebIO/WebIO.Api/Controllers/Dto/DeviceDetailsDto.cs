namespace WebIO.Api.Controllers.Dto;

public record DeviceDetailsDto
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required DeviceTypeDto DeviceType { get; init; }

    public List<InterfaceInfoDto> Interfaces { get; init; } = new();

    public required string Comment { get; init; }
}