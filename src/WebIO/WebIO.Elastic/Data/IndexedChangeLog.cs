namespace WebIO.Elastic.Data;

using Management;

public record IndexedChangeLog : IIndexedEntity<Guid>
{
  public Guid Id { get; init; } = Guid.NewGuid();
  public DateTime Timestamp = DateTime.Now;
  public required string Username { get; init; }
  public required string Comment { get; init; }
  public required string Summary { get; init; }
  public required string FullInfo { get; init; }
}