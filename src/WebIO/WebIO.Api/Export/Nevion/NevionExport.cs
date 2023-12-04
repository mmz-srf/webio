namespace WebIO.Api.Export.Nevion;

using System.IO.Compression;
using Api.Nevion;
using Application;
using DataAccess;
using Microsoft.Extensions.Logging;
using Model;
using Model.Display;
using MoreLinq;
using Stream = Stream;

// Instantiated by name!
// ReSharper disable once UnusedType.Global
public class NevionExport : Export, IExport
{
  private readonly IExportConfigurationRepository _exportConfigurations;
  private readonly ILogger<NevionExport> _log;

  public string Name => "Nevion";

  public string DisplayName => "Nevion";

  public NevionExport(
    IDeviceRepository deviceRepository,
    IExportConfigurationRepository exportConfigurations,
    IMetadataRepository metadata,
    NevionApi api,
    ILogger<NevionExport> log) : base(deviceRepository, metadata, api, log)
  {
    _exportConfigurations = exportConfigurations;
    _log = log;
  }

  public ExportResult Export(ExportArgs exportArgs)
  {
    _log.LogInformation("Generating Nevion Export");
    var now = DateTime.Now;

    var interfaceInfos = GetAllInterfaces(exportArgs);
    var streamInfos = GetAllStreams(exportArgs);

    //_log.Debug("Writing I/O Worksheet");
    _log.LogInformation("Done Generating Nevion Export");

    var zipStream = new MemoryStream();
    {
      using var z = new ZipArchive(zipStream, ZipArchiveMode.Update, leaveOpen: true);
      _log.LogDebug("Writing Inventory");
      var inventory = z.CreateEntry($@"Nevion\Nevion_Inventory_{now:yyyyMMddHHmm}.csv");
      using (var output = inventory.Open())
      {
        ExportData(interfaceInfos, _exportConfigurations.NevionBom, output);
      }

      _log.LogDebug("Writing I/O Worksheet");
      var ios = z.CreateEntry($@"Nevion\Nevion_ios_{now:yyyyMMddHHmm}.csv");
      using (var output = ios.Open())
      {
        ExportData(streamInfos, _exportConfigurations.NevionIo, output);
      }

      _log.LogDebug("Writing Nevion Export");
      var nevion = z.CreateEntry($@"Nevion\Nevion_{now:yyyyMMddHHmm}.csv");
      using (var output = nevion.Open())
      {
        ExportData(streamInfos, _exportConfigurations.Nevion, output);
      }
    }

    zipStream.Seek(0, SeekOrigin.Begin);
    return ExportResult.Create(zipStream, ExportFileType.Zip, $"Nevion_{now:yyyyMMddHHmm}.zip");
  }

  public ExportResult Export(Dictionary<string, string> request)
  {
    _log.LogInformation("Generating Nevion Export");
    var now = DateTime.Now;

    var interfaceInfos = GetAllInterfaces(request);
    var streamInfos = GetAllStreams(request);

    _log.LogInformation("Done Generating Nevion Export");

    var zipStream = new MemoryStream();
    {
      using var z = new ZipArchive(zipStream, ZipArchiveMode.Update, leaveOpen: true);

      _log.LogDebug("Writing Inventory");
      var inventory = z.CreateEntry($@"Nevion\Nevion_Inventory_{now:yyyyMMddHHmm}.csv");
      using (var output = inventory.Open())
      {
        ExportData(interfaceInfos, _exportConfigurations.NevionBom, output);
      }

      _log.LogDebug("Writing I/O Worksheet");
      var ios = z.CreateEntry($@"Nevion\Nevion_ios_{now:yyyyMMddHHmm}.csv");
      using (var output = ios.Open())
      {
        ExportData(streamInfos, _exportConfigurations.NevionIo, output);
      }

      _log.LogDebug("Writing Nevion Export");
      var nevion = z.CreateEntry($@"Nevion\Nevion_{now:yyyyMMddHHmm}.csv");
      using (var output = nevion.Open())
      {
        ExportData(streamInfos, _exportConfigurations.Nevion, output);
      }
    }

    zipStream.Seek(0, SeekOrigin.Begin);
    return ExportResult.Create(zipStream, ExportFileType.Zip, $"Nevion_{now:yyyyMMddHHmm}.zip");
  }

  private void ExportData(
    IEnumerable<ValueGetter> dataRow,
    IReadOnlyCollection<ColumnDefinition> columns,
    Stream output)
  {
    var writer = new StreamWriter(output);

    AddHeaders(columns, writer);
    AddDataRows(dataRow, columns, writer);
    writer.Flush();
    output.Seek(0, SeekOrigin.Begin);
  }

  private void AddHeaders(IEnumerable<ColumnDefinition> columns, TextWriter writer)
  {
    columns.ForEach((column, index) =>
    {
      var columnName = GetColumnName(column);
      writer.Write($"{columnName}({index});");
    });
    writer.WriteLine();
  }

  private void AddDataRows(
    IEnumerable<ValueGetter> dataRow,
    IReadOnlyCollection<ColumnDefinition> columns,
    TextWriter writer)
  {
    var rows = dataRow.ToList();
    var dataRowsThatExistInNevion = DataRowsThatExistInNevion(rows).ToList();
    if (dataRowsThatExistInNevion.Any())
    {
      rows = dataRowsThatExistInNevion;
    }

    foreach (var row in rows)
    {
      columns.ForEach((column, _) =>
      {
        var value = GetValue(column, row);
        writer.Write($"{RemoveLineBreaks(value)};");
      });
      writer.WriteLine();
    }
  }

  private static string RemoveLineBreaks(string s)
    => s
      .Replace("\r\n", " ")
      .Replace('\r', ' ')
      .Replace('\n', ' ');

  private static IEnumerable<ValueGetter> DataRowsThatExistInNevion(IEnumerable<ValueGetter> dataRow)
    => dataRow.Where(r
      => !string.IsNullOrWhiteSpace(GetValue(new() {Property = PseudoProperties.Channel},
        r)));
}