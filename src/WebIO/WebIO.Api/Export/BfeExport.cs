namespace WebIO.Api.Export;

using Application;
using Controllers;
using DataAccess;
using Elastic.Data;
using Microsoft.Extensions.Logging;
using Model.Display;
using MoreLinq;
using OfficeOpenXml;

// Instantiated by name!
// ReSharper disable once UnusedType.Global
public class BfeExport : Export, IExport
{
  private readonly IExportConfigurationRepository _exportConfigurations;
  private readonly ILogger _log;

  public string Name => "BFE";

  public string DisplayName => "BFE";

  public BfeExport(
    IDeviceRepository deviceRepository,
    IExportConfigurationRepository exportConfigurations,
    IMetadataRepository metadata,
    ILogger<BfeExport> log) : base(deviceRepository, metadata, null, log) // TODO
  {
    _exportConfigurations = exportConfigurations;
    _log = log;
  }

  public async Task<ExportResult> Export(ExportArgs exportArgs, CancellationToken ct)
  {
    _log.LogInformation("Generating BFE Export");

    var stream = new MemoryStream();
    using var p = new ExcelPackage();

    var now = DateTime.Now;
    var filename = $"BFE_{now:yyyyMMdd_HHmm}.xlsx";

    var interfaceInfos = await GetAllInterfacesAsync(exportArgs, ct);
    ExportInterfaces(interfaceInfos, p, "KscAcquisition", "Inventory_BFE_KSC_Acq");
    ExportInterfaces(interfaceInfos, p, "KscAggregation", "Inventory_BFE_KSC_Agg");
    ExportInterfaces(interfaceInfos, p, "KscProduction", "Inventory_BFE_KSC_Pro");

    var streamInfos = await GetAllStreamsAsync(exportArgs, ct);
    ExportStreams(streamInfos, p, "KscAcquisition", StreamDirection.Send, "I-O_BFE_KSC_Acq_Sender");
    ExportStreams(streamInfos, p, "KscAcquisition", StreamDirection.Receive, "I-O_BFE_KSC_Acq_Receiver");
    ExportStreams(streamInfos, p, "KscAggregation", StreamDirection.Send, "I-O_BFE_KSC_Agg_Sender");
    ExportStreams(streamInfos, p, "KscAggregation", StreamDirection.Receive, "I-O_BFE_KSC_Agg_Receiver");
    ExportStreams(streamInfos, p, "KscProduction", StreamDirection.Send, "I-O_BFE_KSC_Pro_Sender");
    ExportStreams(streamInfos, p, "KscProduction", StreamDirection.Receive, "I-O_BFE_KSC_Pro_Receiver");

    _log.LogDebug("Saving Document");
    p.SaveAs(stream);
    stream.Seek(0, SeekOrigin.Begin);

    _log.LogInformation("Done Generating BFE Export");
    return ExportResult.Create(stream, ExportFileType.Excel, filename);
  }

  public async Task<ExportResult> Export(Dictionary<string, string> request, CancellationToken ct)
  {
    _log.LogInformation("Generating BFE Export");

    var stream = new MemoryStream();
    using var p = new ExcelPackage();
    var now = DateTime.Now;
    var filename = $"BFE_{now:yyyyMMdd_HHmm}.xlsx";

    var interfaceInfos = await GetAllInterfacesAsync(request, ct);
    ExportInterfaces(interfaceInfos, p, "KscAcquisition", "Inventory_BFE_KSC_Acq");
    ExportInterfaces(interfaceInfos, p, "KscAggregation", "Inventory_BFE_KSC_Agg");
    ExportInterfaces(interfaceInfos, p, "KscProduction", "Inventory_BFE_KSC_Pro");

    var streamInfos = await GetAllStreamsAsync(request, ct);
    ExportStreams(streamInfos, p, "KscAcquisition", StreamDirection.Send, "I-O_BFE_KSC_Acq_Sender");
    ExportStreams(streamInfos, p, "KscAcquisition", StreamDirection.Receive, "I-O_BFE_KSC_Acq_Receiver");
    ExportStreams(streamInfos, p, "KscAggregation", StreamDirection.Send, "I-O_BFE_KSC_Agg_Sender");
    ExportStreams(streamInfos, p, "KscAggregation", StreamDirection.Receive, "I-O_BFE_KSC_Agg_Receiver");
    ExportStreams(streamInfos, p, "KscProduction", StreamDirection.Send, "I-O_BFE_KSC_Pro_Sender");
    ExportStreams(streamInfos, p, "KscProduction", StreamDirection.Receive, "I-O_BFE_KSC_Pro_Receiver");

    _log.LogDebug("Saving Document");
    p.SaveAs(stream);
    stream.Seek(0, SeekOrigin.Begin);

    _log.LogInformation("Done Generating BFE Export");
    return ExportResult.Create(stream, ExportFileType.Excel, filename);
  }

  private void ExportInterfaces(
    IEnumerable<InterfaceValueGetter> interfaceInfos,
    ExcelPackage p,
    string filterKey,
    string worksheetName)
  {
    ExportData(
      interfaceInfos.Where(i => i.RepresentsTrue(filterKey)),
      p.Workbook.Worksheets.Add(worksheetName),
      _exportConfigurations.BfeBom);
  }

  private void ExportStreams(
    IEnumerable<StreamsValueGetter> streamInfos,
    ExcelPackage p,
    string filterKey,
    StreamDirection direction,
    string worksheetName)
  {
    ExportData(
      streamInfos
        .Where(i => i.RepresentsTrue(filterKey))
        .Where(i => i.Stream.Direction == direction),
      p.Workbook.Worksheets.Add(worksheetName),
      _exportConfigurations.BfeBom);
  }

  private void ExportData(
    IEnumerable<ValueGetter> dataRows,
    ExcelWorksheet worksheet,
    IReadOnlyCollection<ColumnDefinition> columns)
  {
    _log.LogDebug("Writing Worksheet {Name}", worksheet.Name);
    worksheet.Cells[1, 1].Value = $"{DateTime.Now:F}";

    var currentRow = 3;
    foreach (var row in dataRows)
    {
      columns.ForEach((column, index) => { worksheet.Cells[currentRow, index + 1].Value = GetValue(column, row); });
      currentRow++;
    }

    // headers
    const int headerRow = 2;
    columns.ForEach((column, index) => { worksheet.Cells[headerRow, index + 1].Value = GetColumnName(column); });
  }
}
