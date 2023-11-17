namespace WebIO.Api.Controllers.Dto;

public record InterfaceTemplateDto
{
  public required string Name { get; init; }
  public required string DisplayName { get; init; }
  public List<StreamTemplateDto> Streams { get; init; } = new();
}
