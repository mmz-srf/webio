namespace WebIO.Api.Export;

public record ExportArgs
{
  public required string ExportTargetName { get; init; }
  public bool All { get; set; }
  public IEnumerable<string> SelectedDeviceIds { get; init; } = new List<string>();
  public Dictionary<string, string> Filters { get; init; } = new();
}
