namespace WebIO.Api.Export;

using Microsoft.Extensions.DependencyInjection;

public class ExportFactory : IExportFactory
{
  private readonly IServiceProvider _provider;

  public ExportFactory(IServiceProvider provider)
  {
    _provider = provider;
  }

  public IExport GetExport(string exportType)
    => _provider.GetServices<IExport>()
         .FirstOrDefault(e => string.Equals(e.Name, exportType)) 
       ?? throw new ArgumentException("Invalid export type", exportType);

  public IEnumerable<ExportType> AvailableTypes =>
    _provider.GetServices<IExport>()
      .Select(e => new ExportType(e.Name, e.DisplayName))
      .ToList();
}
