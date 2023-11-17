namespace WebIO.Api.Export;

public interface IExportFactory
{
    IExport GetExport(string exportType);
    IEnumerable<ExportType> AvailableTypes { get; }
}