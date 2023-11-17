namespace WebIO.Elastic.Data;

public record Changes
{
  public DateTime Created { get; init; } = DateTime.Now;
  public required string Creator { get; init; }
  public DateTime Modified { get; init; } = DateTime.Now;
  public required string Modifier { get; init; }
}
