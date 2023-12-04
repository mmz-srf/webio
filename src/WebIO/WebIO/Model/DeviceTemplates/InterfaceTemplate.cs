namespace WebIO.Model.DeviceTemplates;

using System.Collections.Generic;

public class InterfaceTemplate
{
    public required string Name { get; init; }

    public required string DisplayName { get; init; }

    public List<TemplateFieldValue> FieldValues { get; set; } = new();

    public List<StreamTemplate> Streams { get; set; } = new();
}

public class InterfaceDefinition
{
    public required string Template { get; init; }

    public int Count { get; init; }
}