namespace WebIO.Elastic.Data;

using System.Collections.Immutable;
using Management;
using Nest;

public record IndexedInterface : IIndexedEntity<Guid>
{
  public Guid Id { get; init; } = Guid.NewGuid();
  [Keyword]
  public Guid DeviceId { get; init; }
  [Keyword]
  public string Name { get; init; } = string.Empty;
  public int Index { get; init; }
  [Keyword]
  public string InterfaceTemplate { get; init; } = string.Empty;
  public string Comment { get; init; } = string.Empty;
  [Keyword]
  public string DeviceType { get; init; } = string.Empty;
  [Keyword]
  public string DeviceName { get; init; } = string.Empty;

  [Nested]
  public IReadOnlyDictionary<string, string> InterfaceProperties { get; init; } = ImmutableDictionary<string, string>.Empty;

  public int StreamsCountVideoSend { get; init; }
  public int StreamsCountAudioSend { get; init; }
  public int StreamsCountAncillarySend { get; init; }
  public int StreamsCountVideoReceive { get; init; }
  public int StreamsCountAudioReceive { get; init; }
  public int StreamsCountAncillaryReceive { get; init; }

  [Nested]
  public IReadOnlyDictionary<string, string> DeviceProperties { get; init; } =
    ImmutableDictionary<string, string>.Empty;
}
