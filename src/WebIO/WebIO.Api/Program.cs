using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebIO.Api;
using WebIO.Application;
using WebIO.DataAccess;

var builder = WebApp.CreateWebApplicationBuilder(args);
var webApp = builder.Build();

WebApp.ConfigureWebApp(webApp, builder);

Initialize(webApp);
webApp.Run();
return 0;

void Initialize(IHost webHost)
{
  using var scope = webHost.Services.CreateScope();
  var scripts = new ScriptHelper(scope.ServiceProvider.GetRequiredService<ILogger<ScriptHelper>>());
  var configReader = scope.ServiceProvider.GetRequiredService<IConfigFileReader>();

  if (scope.ServiceProvider.GetService<IMetadataRepository>() is ICanBeReloaded metadata)
  {
    metadata.Reload(configReader, scripts);
  }

  if (scope.ServiceProvider.GetService<IExportConfigurationRepository>() is ICanBeReloaded exports)
  {
    exports.Reload(configReader, scripts);
  }
}
