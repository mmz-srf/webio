using System.Reflection;
using CommandLine;
using WebIO;
using WebIO.Cli;
using static WebIO.Cli.CliApp;

var builder = CreateAppBuilder(args);
builder.Services.Setup();
var webApp = builder.Build();

return Parser.Default.ParseArguments<InitOptions, ImportOptions, ReindexAllOptions, VersionOptions>(args)
  .MapResult(
    (InitOptions _) => HandleErrors(() => InitializeSchema(webApp)),
    (ImportOptions opt) => HandleErrors(() => Import(webApp, opt.ImportFile)),
    (ReindexAllOptions _) => HandleErrors(() => ReindexAll(webApp)),
    (VersionOptions _) => HandleErrors(PrintVersion),
    _ => 1);

int HandleErrors(Action action)
{
  try
  {
    action();
    return 0;
  }
  catch (Exception e)
  {
    Console.WriteLine($"Unhandled error: {e.Message}");
    return 1;
  }
}

void PrintVersion()
  => Console.WriteLine($"{App.AppName} v{Assembly.GetEntryAssembly()!.GetName().Version}");