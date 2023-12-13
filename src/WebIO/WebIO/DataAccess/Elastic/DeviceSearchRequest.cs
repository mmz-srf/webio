namespace WebIO.DataAccess.Elastic;

using System.Collections.Generic;
using System.Collections.Immutable;
using WebIO.Elastic.Management.Search;

public record DeviceSearchRequest : SearchRequest
{
  public string DeviceName { get; init; } = string.Empty;
  public IReadOnlyDictionary<string, string> Properties { get; init; } = ImmutableDictionary<string, string>.Empty;
}
