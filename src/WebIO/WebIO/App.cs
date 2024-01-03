namespace WebIO;

using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public static class App
{
  public const string AppName = "WebIO";

  public static void RegisterConfiguration(IConfigurationBuilder config, IHostEnvironment env, string[] args)
  {
    config
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
      .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true,
        reloadOnChange: true)
      .AddEnvironmentVariables()
      .AddCommandLine(args);
  }

  public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration config)
    => services
      .AddHttpClient()
      .SetupDependencies(config);
}
