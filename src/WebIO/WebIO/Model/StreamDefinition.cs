namespace WebIO.Model;

using System.Collections.Generic;

public record StreamDefinition
{
  public string DeviceTypeName { get; init; } = string.Empty;
  public List<StreamMapping> Mappings { get; init; } = new();
}
