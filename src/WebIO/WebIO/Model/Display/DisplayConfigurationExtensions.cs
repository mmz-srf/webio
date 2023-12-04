namespace WebIO.Model.Display;

using System;
using System.Collections.Generic;

public static class DisplayConfigurationExtensions
{
  public static IEnumerable<ColumnGroup> ColumnsFor(this DisplayConfiguration config, FieldLevel level)
    => level switch
    {
      FieldLevel.Device => config.DeviceColumns,
      FieldLevel.Interface => config.InterfaceColumns,
      FieldLevel.Stream => config.StreamColumns,
      _ => throw new ArgumentOutOfRangeException(nameof(level), level, null),
    };
}
