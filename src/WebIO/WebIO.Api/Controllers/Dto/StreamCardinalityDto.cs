namespace WebIO.Api.Controllers.Dto;

public record StreamCardinalityDto
{
  public int AudioSend { get; init; }
  public int AudioReceive { get; init; }
  public int VideoSend { get; init; }
  public int VideoReceive { get; init; }
  public int AncillarySend { get; init; }
  public int AncillaryReceive { get; init; }
}
