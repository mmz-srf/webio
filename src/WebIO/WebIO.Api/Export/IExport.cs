namespace WebIO.Api.Export;

public interface IExport
{
    Task<ExportResult> Export(ExportArgs exportArgs, CancellationToken ct);

    string Name { get; }
    string DisplayName { get; }
}