namespace WebIO.Cli.ExcelImport;

using System.Text.RegularExpressions;
using DataAccess;
using Elastic.Data;
using Extensions;
using Microsoft.Extensions.Logging;
using Model;
using Model.DeviceTemplates;
using MoreLinq.Extensions;
using Newtonsoft.Json;
using OfficeOpenXml;

public partial class ExcelImport
{
  private const string ExampleRowFontColor = "FFFF0000";

  private static readonly Regex StreamNamePattern = StreamNameRegex();

  private static readonly Regex StreamNamePatternA = StreamNameRegexA();

  public record StreamsDataRow
  {
    public StreamsDataRow(Dictionary<string, string>? columns = null)
    {
      Columns = columns ?? new Dictionary<string, string>();
    }

    public required string StreamName { get; init; }
    public Dictionary<string, string> Columns { get; } = new();
    public required StreamType Type { get; init; }
    public StreamDirection Direction { get; init; } = StreamDirection.Send;
    public required string DeviceName { get; init; }
    public int InterfaceIndex { get; init; }
    public int StreamIndex { get; init; }
    public string Suffix { get; init; } = string.Empty;
  }

  public record BomDataRow
  {
    public BomDataRow(Dictionary<string, string>? columns = null)
    {
      Columns = columns ?? new Dictionary<string, string>();
    }

    public string? InterfaceName { get; init; }

    public int InterfaceIndex { get; init; }

    public IDictionary<string, string> Columns { get; }

    public string? OldId { get; init; }
  }

  private readonly IDeviceRepository _deviceRepository;
  private readonly IMetadataRepository _metadataRepository;
  private readonly ILogger _log;
  private readonly DeviceCreation _deviceCreation;

  public ExcelImport(
    IDeviceRepository deviceRepository,
    IMetadataRepository metadataRepository,
    ILogger<ExcelImport> log)
  {
    _deviceRepository = deviceRepository;
    _metadataRepository = metadataRepository;
    _log = log;

    _deviceCreation = new(_metadataRepository, log);
  }

  public async Task Import(string filename, CancellationToken ct)
  {
    var deviceFields = _metadataRepository.DataFields.Where(f => f.EditLevels.Contains(FieldLevel.Device)).ToList();
    var interfaceFields = _metadataRepository.DataFields.Where(f => f.EditLevels.Contains(FieldLevel.Interface))
      .ToList();
    var streamFields = _metadataRepository.DataFields.Where(f => f.EditLevels.Contains(FieldLevel.Stream)).ToList();
    var modification = new ModifyArgs("import", DateTime.Now, "import");

    _log.LogInformation("Import from {File}", filename);
    var manifest = ReadManifest(filename);

    if (manifest == null)
      return;

    foreach (var deviceTypeManifest in manifest.DeviceTypes)
    {
      var importFiles = deviceTypeManifest.Files
        .SelectMany(f => Directory.GetFiles(manifest.BaseDirectory!, f))
        .ToList();

      var fileTypeManifest = manifest.FileTypes.FirstOrDefault(t => t.Name == deviceTypeManifest.FileType);
      if (fileTypeManifest == null)
      {
        fileTypeManifest = manifest.FileTypes.First();
        _log.LogWarning("Unknown file type {DeviceType} using {DefaultDeviceType}",
          deviceTypeManifest.FileType,
          fileTypeManifest.Name);
      }

      var deviceType =
        _metadataRepository.DeviceTypes.FirstOrDefault(t => t.Name == deviceTypeManifest.DeviceTypeName);

      if (deviceType == null)
      {
        continue; // todo: refactor
      }

      foreach (var importFile in importFiles)
      {
        var file = new FileInfo(importFile);
        if (!file.Exists)
        {
          _log.LogError("File {Filename} not found. Skipping", file.Name);
          continue;
        }

        using var p = new ExcelPackage(file);
        _log.LogInformation("Reading import data from {Filename}", importFile);

        var bom = ReadBom(fileTypeManifest.Bom, p);
        _log.LogInformation("Reading bill of material Done. {LineCount} lines read", bom.Count);

        var streams = ReadStreams(fileTypeManifest.Streams, p);
        _log.LogInformation("Reading streams Done. {LineCount} lines read", streams.Count);

        var deviceName = streams.FirstOrDefault()?.DeviceName
                         ?? bom.FirstOrDefault()?.InterfaceName
                         ?? string.Empty;

        var interfaceDefinitions = FindInterfaceTemplatesBasedOnImportedInterfaces(deviceType!, bom, streams);

        var device =
          _deviceCreation.CreateDevice(deviceType!, modification, deviceName, "import", interfaceDefinitions);
        ImportValues(deviceFields, bom.First().Columns, device.Properties);

        device.Interfaces.ForEach(iface =>
        {
          var bomRow = bom.FirstOrDefault(row => row.InterfaceIndex == iface.Index);
          if (bomRow is null)
          {
            _log.LogWarning("Interface {Interface} has no matching row in imported excel file", iface.Name);
            return;
          }

          ImportValues(interfaceFields, bomRow.Columns, iface.Properties);

          ImportStreamsOfType(streams, iface, StreamDirection.Send, StreamType.Video, streamFields);
          ImportStreamsOfType(streams, iface, StreamDirection.Receive, StreamType.Video, streamFields);
          ImportStreamsOfType(streams, iface, StreamDirection.Send, StreamType.Audio, streamFields);
          ImportStreamsOfType(streams, iface, StreamDirection.Receive, StreamType.Audio, streamFields);
          ImportStreamsOfType(streams, iface, StreamDirection.Send, StreamType.Ancillary, streamFields);
          ImportStreamsOfType(streams, iface, StreamDirection.Receive, StreamType.Ancillary, streamFields);
        });
        await _deviceRepository.UpsertAsync(device, ct);
      }
    }
  }

  private void ImportStreamsOfType(
    List<StreamsDataRow> streams,
    Interface @interface,
    StreamDirection direction,
    StreamType type,
    List<DataField> streamFields)
  {
    var streamRows = streams
      .Where(s => s.InterfaceIndex == @interface.Index)
      .Where(s => s.Direction == direction)
      .Where(s => s.Type == type)
      .ToList();

    @interface.GetStreams(type, direction).ForEach((stream, streamIndex) =>
    {
      var streamRow = streamRows.ElementAtOrDefault(streamIndex);
      if (streamRow is null)
      {
        _log.LogWarning("Stream {Stream} has no matching row in imported excel file", stream.Name);
        return;
      }

      MergeTags(streamRow);
      ImportValues(streamFields, streamRow.Columns, stream.Properties);
    });
  }

  private IEnumerable<InterfaceTemplateSelection> FindInterfaceTemplatesBasedOnImportedInterfaces(
    DeviceType deviceType,
    List<BomDataRow> bomRows,
    List<StreamsDataRow> streams)
  {
    if (deviceType.InterfaceTemplates.Count == 1)
    {
      return OnlyOneInterfacePresentPreset(deviceType, bomRows, streams);
    }

    if (deviceType.Name == "SnpGateway")
    {
      return SnpGatewaySpecialCasePreset(deviceType, bomRows);
    }

    return RecSendForInterfacesWithSendersPreset(deviceType, bomRows, streams);
  }

  private static IEnumerable<InterfaceTemplateSelection> OnlyOneInterfacePresentPreset(
    DeviceType deviceType,
    IReadOnlyCollection<BomDataRow> bomRows,
    IReadOnlyCollection<StreamsDataRow> streams)
  {
    var template = deviceType.InterfaceTemplates.Single();
    var interfaceCount = deviceType.SoftwareDefinedInterfaceCount ? bomRows.Count : deviceType.InterfaceCount;

    if (deviceType.InterfaceStreamsFlexible)
    {
      return Enumerable.Range(1, interfaceCount)
        .Select(i =>
        {
          var bomDataRow = bomRows.ElementAtOrDefault(i - 1);
          if (bomDataRow is null)
          {
            throw new NotImplementedException();
            // return default cardinality
          }

          return new InterfaceTemplateSelection(template,
            GetStreamCardinality(bomDataRow.InterfaceIndex, streams));
        })
        .ToList();
    }

    return Enumerable.Repeat(new InterfaceTemplateSelection(template), interfaceCount).ToList();
  }

  private IEnumerable<InterfaceTemplateSelection> SnpGatewaySpecialCasePreset(
    DeviceType deviceType,
    List<BomDataRow> bomRows)
  {
    var sendTemplate = deviceType.InterfaceTemplates.FirstOrDefault(t => t.Name == "send");
    var receiveTemplate = deviceType.InterfaceTemplates.FirstOrDefault(t => t.Name == "receive");
    return bomRows
      .Select(row =>
      {
        var portDirection = row.Columns.ValueOrDefault("PortDirection");

        switch (portDirection)
        {
          case "IP>SDI":
            return receiveTemplate;
          case "SDI>IP":
            return sendTemplate;
          default:
            _log.LogWarning("Unknown PortDirection {Value}", portDirection);
            return sendTemplate;
        }
      })
      .Select(t => new InterfaceTemplateSelection(t!))
      .ToList();
  }

  private static IEnumerable<InterfaceTemplateSelection> RecSendForInterfacesWithSendersPreset(
    DeviceType deviceType,
    List<BomDataRow> bomRows,
    List<StreamsDataRow> streams)
  {
    var receiveTemplate = deviceType.InterfaceTemplates.FirstOrDefault(t => t.Name == "receive");
    var recSendTemplate = deviceType.InterfaceTemplates.FirstOrDefault(t => t.Name == "recsend");
    return bomRows
      .Select((row, idx) =>
      {
        var ifaceStreams = streams.Where(s => s.InterfaceIndex == idx + 1).ToList();
        return ifaceStreams.Any(s => s.Direction == StreamDirection.Send)
          ? recSendTemplate
          : receiveTemplate;
      })
      .Select(t => new InterfaceTemplateSelection(t!))
      .ToList();
  }

  private static StreamCardinality GetStreamCardinality(int interfaceIndex, IEnumerable<StreamsDataRow> streams)
  {
    var streamRows = streams
      .Where(s => s.InterfaceIndex == interfaceIndex)
      .ToList();

    return new(
      VideoSend: streamRows.Count(s => s is {Direction: StreamDirection.Send, Type: StreamType.Video}),
      VideoReceive: streamRows.Count(s => s is {Direction: StreamDirection.Receive, Type: StreamType.Video}),
      AudioSend: streamRows.Count(s => s is {Direction: StreamDirection.Send, Type: StreamType.Audio}),
      AudioReceive: streamRows.Count(s => s is {Direction: StreamDirection.Receive, Type: StreamType.Audio}),
      AncillarySend: streamRows.Count(s => s is {Direction: StreamDirection.Send, Type: StreamType.Ancillary}),
      AncillaryReceive: streamRows.Count(
        s => s is {Direction: StreamDirection.Receive, Type: StreamType.Ancillary}));
  }

  private static void ImportValues(
    IEnumerable<DataField> fields,
    IDictionary<string, string> bomValues,
    FieldValues properties)
  {
    foreach (var deviceField in fields)
    {
      if (bomValues.TryGetValue(deviceField.Key, out var value) &&
          !string.IsNullOrWhiteSpace(value))
      {
        if (deviceField.FieldType == DataFieldType.Boolean)
        {
          value = RepresentsTrue(value) ? "true" : "false";
        }

        properties[deviceField] = value;
      }
    }
  }

  private ImportManifest? ReadManifest(string filename)
  {
    var fileInfo = new FileInfo(filename);
    if (!fileInfo.Exists)
    {
      _log.LogError("Import manifest {Filename} not found", filename);
      return null;
    }

    var json = File.ReadAllText(filename);
    var manifest = JsonConvert.DeserializeObject<ImportManifest>(json)! with
    {
      BaseDirectory = fileInfo.DirectoryName,
    };

    return manifest;
  }

  private void MergeTags(StreamsDataRow streamsDataRow)
  {
    var fieldTags = streamsDataRow.Columns
      .Where(kvp => RepresentsTrue(kvp.Value))
      .Where(k => _metadataRepository.Tags.Any(t => t.Name.Equals(k.Key)))
      .Select(kvp => kvp.Key)
      .ToList();

    streamsDataRow.Columns.TryAdd("Tags", string.Empty);
    streamsDataRow.Columns["Tags"] = string.Join(';', fieldTags);
  }

  private static bool RepresentsTrue(string? value)
    => value?.ToLower() switch
    {
      "1" => true,
      "yes" => true,
      "ja" => true,
      "x" => true,
      "true" => true,
      _ => false,
    };

  private List<Dictionary<string, string>> Read(WorksheetImport manifest, ExcelPackage p)
  {
    var result = new List<Dictionary<string, string>>();

    var worksheet = p.Workbook.Worksheets[manifest.Worksheet];
    if (worksheet == null)
    {
      _log.LogError("Worksheet {Worksheet} not found", manifest.Worksheet);
      return result;
    }

    var rowIndex = manifest.FirstDataRow;
    while (true)
    {
      var row = new Dictionary<string, string>();

      if (RowIsExample(worksheet, rowIndex))
      {
        rowIndex++;
        continue;
      }

      // read other columns
      foreach (var column in manifest.Columns)
      {
        var cellValue = worksheet.GetCellValue(rowIndex, column.Column);
        if (!string.IsNullOrWhiteSpace(cellValue))
        {
          row[column.Property] = cellValue;
        }
      }

      if (!row.Any())
      {
        break;
      }

      result.Add(row);
      rowIndex++;
    }

    return result;
  }

  private static bool RowIsExample(ExcelWorksheet worksheet, int rowIndex)
    => worksheet.Row(rowIndex).Style?.Font.Color.Rgb == ExampleRowFontColor;

  private List<BomDataRow> ReadBom(WorksheetImport manifest, ExcelPackage p)
  {
    var result = new List<BomDataRow>();
    _log.LogInformation("Reading bill of material from Excel worksheet {Worksheet}", manifest.Worksheet);

    var rows = Read(manifest, p);
    rows.ForEach((row, index) =>
    {
      row.TryGetValue(PseudoProperties.InterfaceName, out var interfaceName);
      row.TryGetValue("OldId", out var oldId);

      var dataRow = new BomDataRow(row)
      {
        OldId = oldId,
        InterfaceName = interfaceName,
        InterfaceIndex = index + 1,
      };
      result.Add(dataRow);
    });

    return result;
  }

  private List<StreamsDataRow> ReadStreams(WorksheetImport manifest, ExcelPackage p)
  {
    var result = new List<StreamsDataRow>();
    _log.LogInformation("Reading streams from Excel worksheet {Worksheet}", manifest.Worksheet);

    var rows = Read(manifest, p);
    foreach (var row in rows)
    {
      row.TryGetValue(PseudoProperties.StreamName, out var streamName);

      if (string.IsNullOrEmpty(streamName))
      {
        continue;
      }

      var match = StreamNamePattern.Match(streamName);
      if (!match.Success)
      {
        match = StreamNamePatternA.Match(streamName);
        if (!match.Success)
        {
          continue;
        }
      }

      int.TryParse(match.Groups["interface_index"].Value, out var interfaceIndex);
      if (interfaceIndex == 0)
      {
        interfaceIndex = 1;
      }

      int.TryParse(match.Groups["stream_index"].Value, out var streamIndex);

      var dataRow = new StreamsDataRow(row)
      {
        DeviceName = match.Groups["device"].Value,
        InterfaceIndex = interfaceIndex,
        StreamIndex = streamIndex,
        StreamName = streamName,
        Type = GetStreamType(match.Groups["stream_type"].Value),
        Direction = GetDirection(match.Groups["stream_direction"].Value),
        Suffix = match.Groups["suffix"].Value,
      };
      result.Add(dataRow);
    }

    return result;
  }

  private StreamType GetStreamType(string typeText)
  {
    switch (typeText)
    {
      case "VID":
        return StreamType.Video;
      case "AUD":
        return StreamType.Audio;
      case "ANC":
        return StreamType.Ancillary;
    }

    _log.LogWarning("Unknown stream type {Type}. Assuming Video", typeText);
    return StreamType.Video;
  }

  private StreamDirection GetDirection(string text)
  {
    switch (text.ToLower())
    {
      case "send":
        return StreamDirection.Send;
      case "rec":
        return StreamDirection.Receive;
    }

    _log.LogWarning("Unknown stream direction {Type}. Assuming Send", text);
    return StreamDirection.Send;
  }

  [GeneratedRegex(
    @"^(?<device>.+)_(.+)(?<interface_index>\d{2})_(?<stream_type>(VID|AUD|ANC))(?<stream_direction>(rec|send))_(?<stream_index>\d{4})(?<suffix>.*)$")]
  private static partial Regex StreamNameRegex();

  [GeneratedRegex(
    "^(?<device>.+)_(.+)(?<interface_index>)_(?<stream_type>(VID|AUD|ANC))(?<stream_direction>(rec|send))_(?<stream_index>\\d{4})(?<suffix>.*)$")]
  private static partial Regex StreamNameRegexA();
}

public static class ExcelExtensions
{
  public static string? GetCellValue(
    this ExcelWorksheet worksheet,
    int rowIndex,
    string columnLetter)
  {
    var address = $"{columnLetter}{rowIndex}";
    return worksheet.Cells[address]?.Value?.ToString();
  }
}
