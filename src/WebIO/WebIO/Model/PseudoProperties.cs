namespace WebIO.Model;

using System.Linq;
using DataAccess;

public class PseudoProperties
{
  public enum PropertyType
  {
    Undefined,
    General,
    Device,
    Interface,
    Stream,
  }

  private readonly IMetadataRepository _repository;

  public PseudoProperties(IMetadataRepository repository)
  {
    _repository = repository;
  }

  // general
  public const string Name = "Name";
  public const string Comment = "Comment";
  public const string Created = "Created";
  public const string Modified = "Modified";
  public const string CompositeName = "CompositeName";
  public const string CompositeNameSeparator = "_";

  // device
  public const string InterfaceCount = "InterfaceCount";
  public const string DeviceType = "DeviceType";
  public const string CombinedDeviceName = "CombinedDeviceName";
  public const string FullQualifiedDomainNameRealTimeA = "FqdnRealtimeA";
  public const string Driver = "Driver";

  // interface
  public const string DeviceName = "DeviceName";
  public const string InterfaceTemplate = "InterfaceTemplate";
  public const string StreamsCountVideoSend = "StreamsCountVideoSend";
  public const string StreamsCountAudioSend = "StreamsCountAudioSend";
  public const string StreamsCountAncillarySend = "StreamsCountAncillarySend";
  public const string StreamsCountVideoReceive = "StreamsCountVideoReceive";
  public const string StreamsCountAudioReceive = "StreamsCountAudioReceive";
  public const string StreamsCountAncillaryReceive = "StreamsCountAncillaryReceive";

  // stream
  public const string InterfaceName = "InterfaceName";
  public const string StreamName = "StreamName";
  public const string Channel = "Channel";
  public const string Slot = "Slot";
  public const string DescriptorOrchestrator = "DescriptorOrchestrator";

  public PropertyType GetPropertyTypeFromString(string name)
  {
    return name switch
    {
      Name => PropertyType.General,
      Comment => PropertyType.General,
      Created => PropertyType.General,
      Modified => PropertyType.General,
      CompositeName => PropertyType.General,
      CompositeNameSeparator => PropertyType.General,

      InterfaceCount => PropertyType.Device,
      DeviceType => PropertyType.Device,
      CombinedDeviceName => PropertyType.Device,
      FullQualifiedDomainNameRealTimeA => PropertyType.Device,
      Driver => PropertyType.Device,

      DeviceName => PropertyType.Interface,
      InterfaceTemplate => PropertyType.Interface,
      StreamsCountVideoSend => PropertyType.Interface,
      StreamsCountAudioSend => PropertyType.Interface,
      StreamsCountAncillarySend => PropertyType.Interface,
      StreamsCountVideoReceive => PropertyType.Interface,
      StreamsCountAudioReceive => PropertyType.Interface,
      StreamsCountAncillaryReceive => PropertyType.Interface,

      InterfaceName => PropertyType.Stream,
      StreamName => PropertyType.Stream,
      Channel => PropertyType.Stream,
      Slot => PropertyType.Stream,

      _ => GetFromDatafieldsJson(name),
    };
  }

  private PropertyType GetFromDatafieldsJson(string name)
    => GetEditLevelsForField(name) switch
    {
      FieldLevel.Device => PropertyType.Device,
      FieldLevel.Interface => PropertyType.Interface,
      FieldLevel.Stream => PropertyType.Stream,
      _ => PropertyType.Undefined,
    };

  private FieldLevel? GetEditLevelsForField(string name)
    => _repository
      .DataFields
      .FirstOrDefault(df => df.Key == name)?
      .EditLevels
      .LastOrDefault();
}