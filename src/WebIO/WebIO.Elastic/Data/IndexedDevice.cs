namespace WebIO.Elastic.Data;

using System.Collections.Immutable;
using Management;

public record IndexedDevice : IIndexedEntity<Guid>
{
  public Guid Id { get; init; } = Guid.NewGuid();
  public string Name { get; init; } = string.Empty;
  public string DeviceType { get; init; } = string.Empty;
  public string Comment { get; init; } = string.Empty;
  public IReadOnlyDictionary<string, string> Properties { get; init; } = ImmutableDictionary<string, string>.Empty;
  public int InterfaceCount { get; init; }
}