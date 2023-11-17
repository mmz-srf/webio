namespace WebIO.Api.Controllers;

using Application;
using DataAccess;
using Dto;
using Model;
using Model.Readonly;

public class InterfaceValueGetter : ValueGetter
{
  private readonly IMetadataRepository _metadataRepository;

  private readonly InterfaceInfo _iface;

  public InterfaceValueGetter(InterfaceInfo iface, IMetadataRepository metadataRepository)
  {
    _metadataRepository = metadataRepository;
    _iface = iface;
  }

  public override string GetValue(string property)
    => GetFieldValueDto(property).Value;

  public FieldValueDto GetFieldValueDto(string property)
  {
    string? value;
    var inherited = false;
    switch (property)
    {
      case PseudoProperties.Name:
        value = _iface.Name;
        break;
      case PseudoProperties.DeviceName:
        value = _iface.DeviceName;
        break;
      case PseudoProperties.CompositeName:
        value = string.Join(PseudoProperties.CompositeNameSeparator, _iface.DeviceName, _iface.Name);
        break;
      case PseudoProperties.DeviceType:
        var deviceType = _metadataRepository.DeviceTypes.FirstOrDefault(d => string.Equals(d.Name, _iface.DeviceType));
        value = deviceType?.DisplayName ?? _iface.DeviceType;
        break;
      case PseudoProperties.Comment:
        value = _iface.Comment;
        break;
      case PseudoProperties.Created:
        value = $"{_iface.Modification?.Created:g}, {_iface.Modification?.Creator}";
        break;
      case PseudoProperties.Modified:
        value = $"{_iface.Modification?.Modified:g}, {_iface.Modification?.Modifier}";
        break;
      case PseudoProperties.StreamsCountVideoSend:
        value = _iface.Streams.VideoSend.ToString();
        break;
      case PseudoProperties.StreamsCountAudioSend:
        value = _iface.Streams.AudioSend.ToString();
        break;
      case PseudoProperties.StreamsCountAncillarySend:
        value = _iface.Streams.AncillarySend.ToString();
        break;
      case PseudoProperties.StreamsCountVideoReceive:
        value = _iface.Streams.VideoReceive.ToString();
        break;
      case PseudoProperties.StreamsCountAudioReceive:
        value = _iface.Streams.AudioReceive.ToString();
        break;
      case PseudoProperties.StreamsCountAncillaryReceive:
        value = _iface.Streams.AncillaryReceive.ToString();
        break;
      default:
        _iface.Properties.TryGetValue(property, out value);
        if (string.IsNullOrWhiteSpace(value))
        {
          _iface.DeviceProperties.TryGetValue(property, out value);
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
}
