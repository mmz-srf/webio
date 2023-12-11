namespace WebIO.Elastic.Data;

using System.Collections.Immutable;
using Management;
using Nest;

public record IndexedStream : IIndexedEntity<Guid>
{
  public Guid Id { get; init; } = Guid.NewGuid();
  [Keyword] public Guid InterfaceId { get; init; }
  [Keyword] public string DeviceName { get; init; } = string.Empty;
  [Keyword] public string InterfaceName { get; init; } = string.Empty;
  [Keyword] public string Name { get; init; } = string.Empty;
  public string Comment { get; init; } = string.Empty;
  [Keyword] public StreamType Type { get; init; }
  [Keyword] public StreamDirection Direction { get; init; }

  [Nested]
  public IReadOnlyDictionary<string, string> Properties { get; init; } = ImmutableDictionary<string, string>.Empty;

  [Keyword] public Guid DeviceId { get; init; }
  [Keyword] public string DeviceType { get; init; } = string.Empty;

  [Nested]
  public IReadOnlyDictionary<string, string> InterfaceProperties { get; init; } =
    ImmutableDictionary<string, string>.Empty;

  [Nested]
  public IReadOnlyDictionary<string, string> DeviceProperties { get; init; } =
    ImmutableDictionary<string, string>.Empty;
}
