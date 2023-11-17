namespace WebIO.Api.Controllers;

using Application;
using DataAccess;
using Dto;
using Model;
using Model.Readonly;
using Nevion;

public class StreamsValueGetter : ValueGetter
{
  private readonly NevionApi? _nevionApi;
  public StreamInfo Stream { get; }

  public StreamsValueGetter(
    StreamInfo stream,
    IMetadataRepository metadataRepository,
    NevionApi? nevionApi = null)
  {
    _metadataRepository = metadataRepository;
    _nevionApi = nevionApi;
    Stream = stream;
  }

  public FieldValueDto GetFieldValueDto(string property)
  {
    string? value;
    var inherited = false;
    switch (property)
    {
      case PseudoProperties.Name:
        value = Stream.Name;
        break;
      case PseudoProperties.InterfaceName:
        value = Stream.InterfaceName;
        break;
      case PseudoProperties.DeviceName:
        value = Stream.DeviceName;
        break;
      case PseudoProperties.CompositeName:
        value = string.Join(PseudoProperties.CompositeNameSeparator, Stream.DeviceName, Stream.InterfaceName,
          Stream.Name);
        break;
      case PseudoProperties.DeviceType:
        var deviceType =
          _metadataRepository.DeviceTypes.FirstOrDefault(d => string.Equals(d.Name, Stream.DeviceType));
        value = deviceType?.DisplayName ?? Stream.DeviceType;
        break;
      case PseudoProperties.Comment:
        value = Stream.Comment;
        break;
      case PseudoProperties.Created:
        value = $"{Stream.Modification?.Created:g}, {Stream.Modification?.Creator}";
        break;
      case PseudoProperties.Modified:
        value = $"{Stream.Modification?.Modified:g}, {Stream.Modification?.Modifier}";
        break;
      case PseudoProperties.CombinedDeviceName:
        var vipDeviceName = _nevionApi?.GetDeviceName(Stream.DeviceName).GetAwaiter().GetResult() ?? "[NA]";
        value = $"{Stream.DeviceName}|{vipDeviceName}";
        break;
      case PseudoProperties.Channel:
        var channelStreamInfo = GetStreamInfo();
        value = $"{channelStreamInfo.DestinationVipChannel}|{channelStreamInfo.DestinationVipStreamLabel}";
        break;
      case PseudoProperties.Slot:
        var slotStreamInfo = GetStreamInfo();
        value = $"{slotStreamInfo.DestinationVipSlot}";

        break;
      default:
        Stream.Properties.TryGetValue(property, out value);
        if (string.IsNullOrWhiteSpace(value))
        {
          Stream.InterfaceProperties.TryGetValue(property, out value);
          inherited = !string.IsNullOrWhiteSpace(value);
        }

        if (string.IsNullOrWhiteSpace(value))
        {
          Stream.DeviceProperties.TryGetValue(property, out value);
          inherited = !string.IsNullOrWhiteSpace(value);
        }

        break;
    }

    return new()
    {
      Value = value ?? string.Empty,
      Inherited = inherited,
    };
  }

  private StreamMapping GetStreamInfo()
  {
    var compositeName = GetValue(PseudoProperties.CompositeName);

    var deviceId = _nevionApi?.GetDeviceName(Stream.DeviceName).GetAwaiter().GetResult() ?? string.Empty;

    var streamMappings = _metadataRepository.NevionStreamDefinitions.SingleOrDefault(i
      => i.DeviceTypeName.Equals(Stream.DeviceType));

    var streamMapping = streamMappings?.Mappings.SingleOrDefault(i
      => i.SourceWebIoStreamName!.Replace("{deviceName}", Stream.DeviceName) ==
         compositeName);

    var streamInfos = _nevionApi?.GetStreamInfos(Stream.DeviceName).GetAwaiter().GetResult();
    var apiInfos = streamInfos?.SingleOrDefault(i
      => i.SourceWebIoStreamName!.Contains(compositeName));
    return apiInfos ?? EmptyMappingForNewDevice(streamMapping, Stream.DeviceName, deviceId);
  }

  private static StreamMapping EmptyMappingForNewDevice(
    StreamMapping? mapping,
    string deviceName,
    string deviceId)
    => mapping != null
      ? mapping with {DestinationVipChannel = GetChannelName(mapping, deviceName, deviceId)}
      : new();

  private static string GetChannelName(
    StreamMapping mapping,
    string deviceName,
    string deviceId)
    => mapping
         .DestinationVipChannel?
         .Replace("{deviceName}", deviceName)
         .Replace("{deviceId}", deviceId)
       ?? string.Empty;

  private readonly IMetadataRepository _metadataRepository;

  public override string GetValue(string property)
    => GetFieldValueDto(property).Value;
}
