namespace WebIO.Api.Controllers.Dto;

public record DeviceTypeDto
{
  public required string Name { get; init; }
  public required string DisplayName { get; init; }
  public int InterfaceCount { get; init; }
  public List<InterfaceTemplateDto> InterfaceTemplates { get; init; } = new();
  public List<string> DefaultInterfaces { get; init; } = new();
  public bool FlexibleStreams { get; init; }

  public required string InterfaceNamePrefix { get; init; }
}
