namespace WebIO.Model.Readonly;

using System;
using System.Collections.Immutable;

/// <summary>
/// lightweight readonly class for <see cref="Device"/>
/// </summary>
public record DeviceInfo(
  Guid Id,
  string Name,
  string DeviceType,
  string Comment,
  ImmutableDictionary<string, string> Properties,
  // ModificationInfo Modification,
  int InterfacesCount);
