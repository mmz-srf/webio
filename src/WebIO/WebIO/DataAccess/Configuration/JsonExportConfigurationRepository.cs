namespace WebIO.DataAccess.Configuration;

using System.Collections.Generic;
using Application;
using ConfigFiles;
using Microsoft.Extensions.Logging;
using Model.Display;

public class JsonExportConfigurationRepository : IExportConfigurationRepository, ICanBeReloaded
{
  private readonly ILogger _log;

  public List<ColumnDefinition> BfeBom { get; private set; } = new();

  public List<ColumnDefinition> BfeIo { get; private set; } = new();

  public List<ColumnDefinition> NevionBom { get; private set; } = new();

  public List<ColumnDefinition> NevionIo { get; private set; } = new();

  public List<ColumnDefinition> Nevion { get; private set; } = new();

  public JsonExportConfigurationRepository(ILogger<JsonExportConfigurationRepository> log)
  {
    _log = log;
  }

  private static List<ColumnDefinition> LoadAndInitialize(
    IConfigFileReader fileReader,
    ScriptHelper scriptHelper,
    string filename,
    bool initScripts)
  {
    var columns = fileReader.ReadFromJsonFile<List<ColumnDefinition>>(filename);
    if (initScripts)
    {
      scriptHelper.InitializeColumnScripts(columns);
    }

    return columns;
  }

  public void Reload(
    IConfigFileReader fileReader,
    ScriptHelper scriptHelper,
    bool initScripts = true)
  {
    _log.LogDebug("Loading export configurations");
    if (!initScripts)
    {
      _log.LogWarning("Skipping initialization of export column scripts!");
    }

    BfeBom = LoadAndInitialize(fileReader, scriptHelper, ConfigFileNames.ExportBfeBomColumns, initScripts);
    BfeIo = LoadAndInitialize(fileReader, scriptHelper, ConfigFileNames.ExportBfeIoColumns, initScripts);
    NevionBom = LoadAndInitialize(fileReader, scriptHelper, ConfigFileNames.ExportNevionBomColumns, initScripts);
    NevionIo = LoadAndInitialize(fileReader, scriptHelper, ConfigFileNames.ExportNevionIoColumns, initScripts);
    Nevion = LoadAndInitialize(fileReader, scriptHelper, ConfigFileNames.ExportNevionColumns, initScripts);
  }
}
