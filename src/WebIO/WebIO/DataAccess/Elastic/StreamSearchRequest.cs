namespace WebIO.DataAccess.Elastic;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using WebIO.Elastic.Management.Search;

public record StreamSearchRequest : SearchRequest
{
  public string DeviceName { get; init; } = string.Empty;
  public string InterfaceName { get; init; } = string.Empty;
  public IEnumerable<Guid> InterfaceIds { get; init; } = new List<Guid>();
  public string StreamName { get; init; } = string.Empty;
  public IReadOnlyDictionary<string, string> Properties { get; init; } = ImmutableDictionary<string, string>.Empty;
}
