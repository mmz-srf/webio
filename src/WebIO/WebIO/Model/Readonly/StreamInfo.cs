namespace WebIO.Model.Readonly;

using System;
using System.Collections.Immutable;
using Elastic.Data;

/// <summary>
/// lightweight readonly class for <see cref="Interface"/>
/// </summary>
public class StreamInfo
{
  public StreamInfo(
    Guid id,
    string name,
    string comment,
    StreamType type,
    StreamDirection direction,
    Guid deviceId,
    string deviceType,
    string deviceName,
    string interfaceName,
    ImmutableDictionary<string, string> properties,
    ImmutableDictionary<string, string> deviceProperties,
    ImmutableDictionary<string, string> interfaceProperties,
    ModificationInfo? modification)
  {
    Id = id;
    Name = name;
    Comment = comment;
    Type = type;
    Direction = direction;
    DeviceId = deviceId;
    DeviceType = deviceType;
    DeviceName = deviceName;
    InterfaceName = interfaceName;
    Properties = properties;
    DeviceProperties = deviceProperties;
    InterfaceProperties = interfaceProperties;
    Modification = modification;
  }

  public Guid Id { get; }

  public string Name { get; }

  public string Comment { get; }

  public StreamType Type { get; }

  public StreamDirection Direction { get; }

  public Guid DeviceId { get; }

  public string DeviceType { get; }

  public string DeviceName { get; }

  public string InterfaceName { get; }

  public ImmutableDictionary<string, string> Properties { get; }

  public ImmutableDictionary<string, string> DeviceProperties { get; }

  public ImmutableDictionary<string, string> InterfaceProperties { get; }

  public ModificationInfo? Modification { get; }
}
