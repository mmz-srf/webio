namespace WebIO.Model.Readonly;

public class StreamsCountInfo
{
  public StreamsCountInfo(int videoSend, int audioSend, int ancillarySend, int videoReceive, int audioReceive, int ancillaryReceive)
  {
    VideoSend = videoSend;
    AudioSend = audioSend;
    AncillarySend = ancillarySend;
    VideoReceive = videoReceive;
    AudioReceive = audioReceive;
    AncillaryReceive = ancillaryReceive;
  }

  public int VideoSend { get; }
  public int AudioSend { get; }
  public int AncillarySend { get; }
  public int VideoReceive { get; }
  public int AudioReceive { get; }
  public int AncillaryReceive { get; }
}
