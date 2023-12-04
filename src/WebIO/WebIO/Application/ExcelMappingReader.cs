namespace WebIO.Application;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Model;
using Model.DeviceTemplates;
using OfficeOpenXml;

public class ExcelMappingReader
{
  private const int SourceNameCol = 1;
  private const int DeviceNameCol = 2;
  private const int SlotCol = 3;
  private const int ChannelCol = 4;

  private readonly Dictionary<string, string?> _deviceNameMapping;

  public ExcelMappingReader(IEnumerable<DeviceType> deviceTypes)
  {
    _deviceNameMapping = deviceTypes.ToDictionary(kv => kv.DisplayName, kv => kv.Name?.ToLowerInvariant());
  }

  public IEnumerable<StreamDefinition> ReadFromFile(string mappingFile)
    => GetWorkSheetsWithoutLegend(new(new FileInfo(mappingFile)))
      .Where(DeviceIsConfigured)
      .Select(ToStreamDefinition);

  private static IEnumerable<ExcelWorksheet> GetWorkSheetsWithoutLegend(ExcelPackage workbook)
    => workbook.Workbook
      .Worksheets
      .Skip(1);

  private bool DeviceIsConfigured(ExcelWorksheet ws)
    => _deviceNameMapping.ContainsKey(ws.Name);

  private StreamDefinition ToStreamDefinition(ExcelWorksheet ws)
    => new()
    {
      DeviceTypeName = _deviceNameMapping[ws.Name] ?? string.Empty,
      Mappings = GetMappings(ws),
    };

  private static List<StreamMapping> GetMappings(ExcelWorksheet ws)
    => GetDataRows(ws)
      .Select(row => ToStreamMapping(ws, row))
      .ToList();

  private static StreamMapping ToStreamMapping(ExcelWorksheet ws, ExcelRow row)
    => new()
    {
      SourceWebIoStreamName = ReplaceTokens(ValueInCell(ws, row, SourceNameCol)),
      DestinationVipStreamKey = "{deviceId}." + ValueInCell(ws, row, SlotCol) + ".{guid}",
      DestinationVipSlot = ReplaceTokens(ValueInCell(ws, row, SlotCol)),
      DestinationVipStreamLabel = ReplaceTokens(ValueInCell(ws, row, ChannelCol)),
      DestinationVipChannel = "*",
    };

  private static string ValueInCell(
    ExcelWorksheet ws,
    ExcelRow row,
    int colIdx)
    => ws.Cells[row.Row, colIdx].Text;

  private static IEnumerable<ExcelRow> GetDataRows(ExcelWorksheet ws)
    => Rows(ws).Skip(1);

  private static IEnumerable<ExcelRow> Rows(ExcelWorksheet ws)
  {
    var start = ws.Dimension.Start;
    var end = ws.Dimension.End;
    for (var row = start.Row; row <= end.Row; row++)
    {
      yield return ws.Row(row);
    }
  }

  private static string ReplaceTokens(string orig)
    => orig
      .Replace("Gerät", "{deviceName}")
      .Replace("DeviceID", "{deviceId}")
      .Replace("*|", string.Empty);
}
