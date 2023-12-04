namespace WebIO.Api.Controllers.Dto;

using Model;

public record DataFieldDto
{
    public required string Key { get; init; }
    public required string Category { get; init; }
    public required string DisplayName { get; init; }
    public required string Placeholder { get; init; }
    public int MaxLength { get; init; }
    public bool Readonly { get; init; }
    public DataFieldTypeDto FieldType { get; init; }
    public List<string> SelectableValues { get; init; } = new();
    public List<SelectableValue> SelectableValuesExt { get; init; } = new();
    public int Size { get; init; }
    public ColumnVisibilityDto Visible { get; init; }
    public required string Description { get; init; }
    public SelectableValuesFactory? SelectableValuesFactory { get; init; }
}