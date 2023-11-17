namespace WebIO.Model.Display;

using System.Collections.Generic;

public class ColumnGroup
{
    public string? Group { get; set; }

    public List<ColumnDefinition> Columns { get; set; } = new();
}