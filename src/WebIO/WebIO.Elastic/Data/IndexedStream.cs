namespace WebIO.Elastic.Data;

using System.Collections.Immutable;
using Management;

public record IndexedStream : IIndexedEntity<Guid>
{
  public Guid Id { get; init; } = Guid.NewGuid();
  public Guid InterfaceId { get; init; }
  public string DeviceName { get; init; } = string.Empty;
  public string InterfaceName { get; init; } = string.Empty;
  public string Name { get; init; } = string.Empty;
  public string Comment { get; init; } = string.Empty;
  public StreamType Type { get; init; }
  public StreamDirection Direction { get; init; }
  public IReadOnlyDictionary<string, string> Properties { get; init; } = ImmutableDictionary<string, string>.Empty;
  public Guid DeviceId { get; init; }
  public string DeviceType { get; init; } = string.Empty;

  public IReadOnlyDictionary<string, string> InterfaceProperties { get; init; } =
    ImmutableDictionary<string, string>.Empty;

  public IReadOnlyDictionary<string, string> DeviceProperties { get; init; } =
    ImmutableDictionary<string, string>.Empty;
}
