namespace WebIO.Model.DeviceTemplates;

using System.Collections.Generic;
using Elastic.Data;

public class StreamTemplate
{
    public int Count { get; init; }

    public string? NameTemplate { get; init; }

    public StreamType Type { get; init; }

    public StreamDirection Direction { get; init; }

    public List<TemplateFieldValue> FieldValues { get; set; } = new();
}