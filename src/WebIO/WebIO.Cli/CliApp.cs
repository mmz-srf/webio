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
  
  public static void Import(IHost host, string importFile)
  {
    using var scope = host.Services.CreateScope();
    var elasticStartup = scope.ServiceProvider.GetRequiredService<IElasticStartup>();
    elasticStartup.InitializeAllIndexes(default).GetAwaiter().GetResult();
    
    var scripts = new ScriptHelper(scope.ServiceProvider.GetRequiredService<ILogger<ScriptHelper>>());
    var configReader = scope.ServiceProvider.GetRequiredService<IConfigFileReader>();
    if (scope.ServiceProvider.GetRequiredService<IMetadataRepository>() is ICanBeReloaded metadata)
    {
      metadata.Reload(configReader, scripts, initScripts: false);
    }

    var importer = scope.ServiceProvider.GetRequiredService<ExcelImport.ExcelImport>();
    importer.Import(importFile);
  }

  public static void ReindexAll(IHost host)
  {
    using var scope = host.Services.CreateScope();
    var reindexer = scope.ServiceProvider.GetRequiredService<IReindexEverything>();
    reindexer.ReindexAllAsync(default).GetAwaiter().GetResult();
  }

  public static void InitializeSchema(IHost webHost)
  {
    using var scope = webHost.Services.CreateScope();
    
    if (scope.ServiceProvider.GetService(typeof(EfCoreDeviceRepository)) is EfCoreDeviceRepository repo)
    {
      repo.InitSchema();
    }
  }

  public static HostApplicationBuilder CreateAppBuilder(string[] args)
  {
    var builder = Host.CreateApplicationBuilder(args);
    App.RegisterConfiguration(builder.Configuration, builder.Environment);
    builder.Logging.AddConfiguration(builder.Configuration);
    builder.Services.ConfigureServices(builder.Configuration);
    return builder;
  }
}
