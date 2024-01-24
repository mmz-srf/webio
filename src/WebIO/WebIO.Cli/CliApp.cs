namespace WebIO.Cli;

using Application;
using DataAccess;
using DataAccess.EntityFrameworkCore;
using Elastic.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public static class CliApp
{
  public static IServiceCollection Setup(this IServiceCollection services)
    => services.AddTransient<ExcelImport.ExcelImport>();
  
  public static async Task Import(IHost host, string importFile, CancellationToken ct)
  {
    using var scope = host.Services.CreateScope();
    var elasticStartup = scope.ServiceProvider.GetRequiredService<IElasticStartup>();
    await elasticStartup.InitializeAllIndexes(ct);
    
    var scripts = new ScriptHelper(scope.ServiceProvider.GetRequiredService<ILogger<ScriptHelper>>());
    var configReader = scope.ServiceProvider.GetRequiredService<IConfigFileReader>();
    if (scope.ServiceProvider.GetRequiredService<IMetadataRepository>() is ICanBeReloaded metadata)
    {
      metadata.Reload(configReader, scripts, initScripts: false);
    }

    var importer = scope.ServiceProvider.GetRequiredService<ExcelImport.ExcelImport>();
    await importer.Import(importFile, ct);
  }

  public static async Task ReindexAll(IHost host, CancellationToken ct)
  {
    using var scope = host.Services.CreateScope();
    var reindexer = scope.ServiceProvider.GetRequiredService<IReindexEverything>();
    await reindexer.ReindexAllAsync(ct);
  }

  public static async Task InitializeSchema(IHost webHost, CancellationToken ct)
  {
    using var scope = webHost.Services.CreateScope();
    
    if (scope.ServiceProvider.GetService(typeof(EfCoreDeviceRepository)) is EfCoreDeviceRepository repo)
    {
      await repo.InitSchema(ct);
    }
  }

  public static HostApplicationBuilder CreateAppBuilder(string[] args)
  {
    var builder = Host.CreateApplicationBuilder(args);
    App.RegisterConfiguration(builder.Configuration, builder.Environment, args);
    builder.Logging.AddConfiguration(builder.Configuration);
    builder.Services.ConfigureServices(builder.Configuration);
    return builder;
  }
}
