namespace WebIO.Api.Controllers.Dto;

public abstract record EntityBaseDto
{
  public required string Id { get; init; }

  public required string DeviceId { get; init; }
        
  public Dictionary<string, FieldValueDto> Properties { get; } = new();
}
