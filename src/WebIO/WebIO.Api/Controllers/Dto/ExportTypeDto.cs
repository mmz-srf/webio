namespace WebIO.Api.Controllers.Dto;

public record ExportTypeDto
{
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
}