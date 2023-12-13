namespace WebIO.Elastic.Data;

using System.Collections.Immutable;
using Management;
using Nest;

public record IndexedDevice : IIndexedEntity<Guid>
{
  public Guid Id { get; init; } = Guid.NewGuid();
  [Keyword] public string Name { get; init; } = string.Empty;
  [Keyword] public string DeviceType { get; init; } = string.Empty;
  public string Comment { get; init; } = string.Empty;

  [Nested]
  public IReadOnlyDictionary<string, string> DeviceProperties { get; init; } = ImmutableDictionary<string, string>.Empty;

  public int InterfaceCount { get; init; }
}
