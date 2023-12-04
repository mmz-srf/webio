// ReSharper disable ClassNeverInstantiated.Global
namespace WebIO.Cli;

using CommandLine;

[Verb("init", HelpText = "Initialize database schema.")]
public record InitOptions;

[Verb("import", HelpText = "Import data according to the given manifest file")]
public record ImportOptions
{
  [Value(0, MetaName = "import json file", Default = "./ExcelImport/ImportData/import.json")]
  public string ImportFile { get; set; } = string.Empty;
}

[Verb("reindex-all", HelpText = "Reindex all data in elasticsearch")]
public record ReindexAllOptions;

[Verb("version", HelpText = "The current version of the CLI")]
public record VersionOptions;