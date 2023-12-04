namespace WebIO.Model.Readonly;

using System;
using System.Collections.Immutable;

/// <summary>
/// lightweight readonly class for <see cref="Interface"/>
/// </summary>
public class InterfaceInfo
{
  public InterfaceInfo(
    Guid id,
    string name,
    string comment,
    Guid deviceId,
    string deviceType,
    string deviceName,
    string interfaceTemplate,
    StreamsCountInfo streams,
    ImmutableDictionary<string, string> properties,
    ImmutableDictionary<string, string> deviceProperties,
    ModificationInfo? modification)
  {
    Id = id;
    Name = name;
    Comment = comment;
    DeviceId = deviceId;
    DeviceType = deviceType;
    DeviceName = deviceName;
    InterfaceTemplate = interfaceTemplate;
    Streams = streams;
    Properties = properties;
    DeviceProperties = deviceProperties;
    Modification = modification;
  }

  public Guid Id { get; }

  public string Name { get; }

  public string Comment { get; }

  public Guid DeviceId { get; }

  public string DeviceName { get; }

  public string DeviceType { get; }

  public string InterfaceTemplate { get; }

  public StreamsCountInfo Streams { get; }

  public ImmutableDictionary<string, string> Properties { get; }

  public ImmutableDictionary<string, string> DeviceProperties { get; }

  public ModificationInfo? Modification { get; }
}
