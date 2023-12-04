namespace WebIO.Elastic.Data;

using Management;

public class IndexedAdminUser : IIndexedEntity<Guid>
{
  public Guid Id { get; init; } = Guid.NewGuid();
  public required string Email { get; init; }
}
