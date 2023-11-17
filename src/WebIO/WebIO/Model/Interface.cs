namespace WebIO.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using Elastic.Data;

/// <summary>
/// Domain Model class for interfaces
/// </summary>
public class Interface : IHaveProperties
{
  public Guid Id { get; set; } = Guid.NewGuid();

  public string Name { get; set; } = string.Empty;

  public int Index { get; init; }

  public string? InterfaceTemplate { get; set; }

  public string Comment { get; set; } = string.Empty;

  public FieldValues Properties { get; init; } = new();

  public List<Stream> Streams { get; init; } = new();

  public Modification Modification { get; init; } = null!;

  public IEnumerable<Stream> GetStreams(StreamType streamType, StreamDirection direction)
    => Streams
      .Where(s => s.Direction == direction)
      .Where(s => s.Type == streamType);

  public StreamCardinality GetStreamCardinality()
    => new(
      VideoSend: GetStreams(StreamType.Video, StreamDirection.Send).Count(),
      AudioSend: GetStreams(StreamType.Audio, StreamDirection.Send).Count(),
      AncillarySend: GetStreams(StreamType.Ancillary, StreamDirection.Send).Count(),
      VideoReceive: GetStreams(StreamType.Video, StreamDirection.Receive).Count(),
      AudioReceive: GetStreams(StreamType.Audio, StreamDirection.Receive).Count(),
      AncillaryReceive: GetStreams(StreamType.Ancillary, StreamDirection.Receive).Count());
}
