namespace WebIO.DataAccess.Elastic;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using WebIO.Elastic.Management.Search;

public record InterfaceSearchRequest : SearchRequest
{
  public Guid? DeviceId { get; init; }
  public string InterfaceName { get; init; } = string.Empty;
  public string DeviceName { get; init; } = string.Empty;
  public IReadOnlyDictionary<string, string> Properties { get; init; } = ImmutableDictionary<string, string>.Empty;
}
