namespace WebIO.Elastic.Data;

using System.Collections.Immutable;
using Management;

public record IndexedStream : IIndexedEntity<Guid>
{
  public Guid Id { get; init; } = Guid.NewGuid();
  public Guid InterfaceId { get; init; }
  public string Name { get; init; } = string.Empty;
  public string Comment { get; init; } = string.Empty;
  public StreamType Type { get; init; }
  public StreamDirection Direction { get; init; }
  public IReadOnlyDictionary<string, string> Properties { get; init; } = ImmutableDictionary<string, string>.Empty;
}
