namespace WebIO.Model.Display;

using System.Collections.Generic;

public class DisplayConfiguration
{
    public List<ColumnGroup> DeviceColumns { get; set; } = new();
    public List<ColumnGroup> InterfaceColumns { get; set; } = new();
    public List<ColumnGroup> StreamColumns { get; set; } = new();
}