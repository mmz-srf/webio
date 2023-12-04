namespace WebIO.Model.DeviceTemplates;

using System.Collections.Generic;

public class DeviceType
{
    public string? Name { get; init; }

    public required string DisplayName { get; init; }

    public bool SoftwareDefinedInterfaceCount { get; init; }

    public int InterfaceCount { get; init; }

    public bool InterfaceStreamsFlexible { get; init; }

    public List<InterfaceDefinition> DefaultInterfaces { get; set; } = new();

    public string? InterfaceNameTemplate { get; init; }
        
    public List<TemplateFieldValue> FieldValues { get; set; } = new();

    public List<InterfaceTemplate> InterfaceTemplates { get; set; } = new();
}