namespace WebIO.Api.Export;

public class ExportType
{
    public ExportType(string name, string displayName)
    {
        Name = name;
        DisplayName = displayName;
    }

    public string Name { get; }
    public string DisplayName { get; }

}