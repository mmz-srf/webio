namespace WebIO.Tests.MockMetadata;

using System.Collections.Generic;
using DataAccess;
using Model;
using Model.DeviceTemplates;
using Model.Display;

public class MockMetadataRepository : IMetadataRepository
{
  public MockDataFields MockDataFields { get; } = new();
  public MockDeviceTypes MockDeviceTypes { get; } = new();

  public IEnumerable<DataField> DataFields => MockDataFields.AllFields;

  public IEnumerable<DeviceType> DeviceTypes => MockDeviceTypes.AllTypes;
  public DisplayConfiguration DisplayConfiguration => new();
  public IEnumerable<Tag> Tags => new List<Tag>();
  public IEnumerable<StreamDefinition> NevionStreamDefinitions { get; } = new List<StreamDefinition>();
  public bool IsDeviceProperty(string field)
    => true;

  public bool IsInterfaceProperty(string field)
    => true;

  public bool IsStreamProperty(string field)
    => true;
}
