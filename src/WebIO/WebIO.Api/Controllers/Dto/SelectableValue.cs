namespace WebIO.Api.Controllers.Dto;

public record SelectableValue
{
  public required string Value { get; init; }
  public required string BackgroundColor { get; init; }
  public required string ForegroundColor { get; init; }
}
