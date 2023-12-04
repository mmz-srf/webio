// ReSharper disable ClassNeverInstantiated.Global
namespace WebIO.Cli.ExcelImport;

using Newtonsoft.Json;

public record ImportManifest
{
  public List<ImportFileTypeManifest> FileTypes { get; init; } = new();
  public List<ImportDeviceTypeManifest> DeviceTypes { get; init; } = new();

  [JsonIgnore] public string? BaseDirectory { get; init; }
}

public record ImportFileTypeManifest
{
  public required string Name { get; init; }
  public required WorksheetImport Bom { get; init; }
  public required WorksheetImport Streams { get; init; }
}

public record ImportDeviceTypeManifest
{
  public required string DeviceTypeName { get; init; }
  public required string FileType { get; init; }
  public List<string> Files { get; init; } = new();
}

public record DeviceListManifest
{
  public required string Filename { get; init; }
  public required string Worksheet { get; init; }

  [JsonIgnore] public FileInfo? File { get; init; }

  public int FirstDataRow { get; init; }

  public required string InterfaceColumn { get; init; }

  public required string DeviceNameColumn { get; init; }

  public required string DeviceTypeColumn { get; init; }

  public required string InterfaceCountColumn { get; init; }
}

public record IoListManifest
{
  public required WorksheetImport Bom { get; init; }
  public required WorksheetImport Streams { get; init; }
  public List<string> Files { get; init; } = new();

  [JsonIgnore] public List<FileInfo> ImportFiles { get; init; } = new();
}

public record WorksheetImport
{
  public required string Worksheet { get; init; }

  public int FirstDataRow { get; init; }

  public List<ImportColumn> Columns { get; init; } = new();
  public required string DeviceLabelColumn { get; init; }
}

public record ImportColumn
{
  public required string Column { get; init; }
  public required string Property { get; init; }
}
