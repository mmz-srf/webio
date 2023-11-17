namespace WebIO.Model;

public record StreamMapping
{
    public string? SourceWebIoStreamName { get; init; }
    public string? DestinationVipStreamKey { get; init; }
    public string? DestinationVipStreamLabel { get; init; }
    public string? DestinationVipChannel { get; init; }
    public string? DestinationVipSlot { get; init; }
    public string? DestinationVipCodecFormat { get; init; }
    public string? DestinationVipCodecVertexType { get; init; }
}