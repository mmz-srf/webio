namespace WebIO.DataAccess.Configuration;

using System.Collections.Generic;
using System.Linq;
using Application;
using ConfigFiles;
using Microsoft.Extensions.Logging;
using Model;
using Model.DeviceTemplates;
using Model.Display;

public class JsonMetadataRepository : IMetadataRepository, ICanBeReloaded
{
  private readonly ILogger _log;

  public IEnumerable<DataField> DataFields { get; private set; } = new List<DataField>();

  public IEnumerable<DeviceType> DeviceTypes { get; private set; } = new List<DeviceType>();

  public DisplayConfiguration DisplayConfiguration { get; private set; } = null!;

  public IEnumerable<Tag> Tags { get; private set; } = new List<Tag>();

  public IEnumerable<StreamDefinition> NevionStreamDefinitions { get; private set; } = new List<StreamDefinition>();

  public JsonMetadataRepository(ILogger<JsonMetadataRepository> log, IConfigFileReader fileReader)
  {
    _log = log;
    Reload(fileReader, null);
  }

  public void Reload(
    IConfigFileReader fileReader,
    ScriptHelper? scriptHelper,
    bool initScripts = true)
  {
    _log.LogDebug("Loading general configuration");

    DataFields = fileReader.ReadFromJsonFile<List<DataField>>(ConfigFileNames.DataFields);
    DeviceTypes = fileReader.ReadFromJsonFile<List<DeviceType>>(ConfigFileNames.DeviceTypes);
    Tags = fileReader.ReadFromJsonFile<List<Tag>>(ConfigFileNames.Tags);
    DisplayConfiguration = fileReader.ReadFromJsonFile<DisplayConfiguration>(ConfigFileNames.Display);
    NevionStreamDefinitions = new ExcelMappingReader(DeviceTypes).ReadFromFile("ConfigFiles/streammapping.xlsx");

    ValidateDeviceTypes(DeviceTypes);

    // if (initScripts)
    // {
    //   scriptHelper.InitializeColumnScripts(DisplayConfiguration.DeviceColumns.SelectMany(group => group.Columns));
    //   scriptHelper.InitializeColumnScripts(DisplayConfiguration.InterfaceColumns.SelectMany(group => group.Columns));
    //   scriptHelper.InitializeColumnScripts(DisplayConfiguration.StreamColumns.SelectMany(group => group.Columns));
    // }
    // else
    // {
    //   _log.LogWarning("Skipping initialization of display column scripts!");
    // }
  }

  private void ValidateDeviceTypes(IEnumerable<DeviceType> deviceTypes)
  {
    foreach (var deviceType in deviceTypes)
    {
      foreach (var interfaceDefinition in deviceType.DefaultInterfaces
                 .Select(interfaceDefinition => new
                 {
                   interfaceDefinition,
                   template = deviceType.InterfaceTemplates.FirstOrDefault(t => t.Name == interfaceDefinition.Template),
                 })
                 .Where(t1 => t1.template is null)
                 .Select(t1 => t1.interfaceDefinition))
      {
        _log.LogError("Invalid DefaultInterface {Interface} in device type {DeviceType}",
          interfaceDefinition.Template, deviceType.Name);
      }
    }
  }
}
