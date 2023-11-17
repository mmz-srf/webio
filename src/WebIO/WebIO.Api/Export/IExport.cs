namespace WebIO.Api.Export;

public interface IExport
{
    ExportResult Export(ExportArgs exportArgs);

    string Name { get; }
    string DisplayName { get; }
}