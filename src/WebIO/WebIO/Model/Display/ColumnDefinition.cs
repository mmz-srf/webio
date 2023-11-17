namespace WebIO.Model.Display;

using System;
using System.Collections.Generic;
using Application;

public class ColumnDefinition
{
  public string? DisplayName { get; set; }
  public string? Property { get; init; }
  public string? Description { get; set; }
  public ColumnVisibility Visible { get; set; } = ColumnVisibility.Expanded;
  public int Width { get; set; }
  public bool? Readonly { get; set; }
  public string? StaticValue { get; set; }
  public List<string> Script { get; set; } = new();
  public Func<ValueGetter, string?>? ScriptDelegate { get; set; }
}
