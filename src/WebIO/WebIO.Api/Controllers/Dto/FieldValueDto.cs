namespace WebIO.Api.Controllers.Dto;

public record FieldValueDto
{
  public string Value { get; init; } = string.Empty;
  public bool Inherited { get; init; } = false;
  public bool Dirty { get; init; }
}
