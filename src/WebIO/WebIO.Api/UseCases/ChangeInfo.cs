namespace WebIO.Api.UseCases;

public record ChangeInfo
{
  public required string Property { get; init; }
  public required string OldValue { get; init; } = "null";
  public required string NewValue { get; init; }
  public required string Entity { get; init; }
  public required string Device { get; init; }
  public required string EntityName { get; init; }
  public required string EntityType { get; init; }
}
