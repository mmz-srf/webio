namespace WebIO.Model;

using System;
using Elastic.Data;

public record StreamCardinality(
  int VideoSend,
  int AudioSend,
  int AncillarySend,
  int VideoReceive,
  int AudioReceive,
  int AncillaryReceive)
{
  public int GetCount(StreamDirection direction, StreamType type)
  {
    return type switch
    {
      StreamType.Video when direction == StreamDirection.Send => VideoSend,
      StreamType.Audio when direction == StreamDirection.Send => AudioSend,
      StreamType.Ancillary when direction == StreamDirection.Send => AncillarySend,
      StreamType.Video when direction == StreamDirection.Receive => VideoReceive,
      StreamType.Audio when direction == StreamDirection.Receive => AudioReceive,
      StreamType.Ancillary when direction == StreamDirection.Receive => AncillaryReceive,
      _ => throw new ArgumentException("Invalid stream type", nameof(type)),
    };
  }
}
