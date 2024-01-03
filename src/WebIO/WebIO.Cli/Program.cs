using System.Reflection;
using CommandLine;
using WebIO;
using WebIO.Cli;
using static WebIO.Cli.CliApp;

var builder = CreateAppBuilder(args);
builder.Services.Setup();
var webApp = builder.Build();

return await Parser.Default.ParseArguments<InitOptions, ImportOptions, ReindexAllOptions, VersionOptions>(args)
  .MapResult(
    (InitOptions _) => HandleErrors(() => InitializeSchema(webApp, default)),
    (ImportOptions opt) => HandleErrors(() => Import(webApp, opt.ImportFile, default)),
    (ReindexAllOptions _) => HandleErrors(() => ReindexAll(webApp, default)),
    (VersionOptions _) => HandleErrors(PrintVersion),
    _ => Task.FromResult(1));

async Task<int> HandleErrors(Func<Task> action)
{
  try
  {
    await action();
    return 0;
  }
  catch (Exception e)
  {
    Console.WriteLine($"Unhandled error: {e.Message}");
    return 1;
  }
}

Task PrintVersion()
{
  Console.WriteLine($"{App.AppName} v{Assembly.GetEntryAssembly()!.GetName().Version}");
  return Task.CompletedTask;
}
