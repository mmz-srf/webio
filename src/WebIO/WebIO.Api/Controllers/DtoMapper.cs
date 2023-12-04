namespace WebIO.Api.Controllers;

using DataAccess;
using Dto;
using Model;
using Model.Display;

public static class DtoMapper
{
  public static IEnumerable<DataFieldDto> MapDisplayConfigurationToDtos(
    IMetadataRepository metadata,
    FieldLevel level)
  {
    var dataFields = metadata.DataFields;
    var columnGroups = metadata.DisplayConfiguration!
      .ColumnsFor(level)
      .SelectMany(g => g.Columns.Select(c => new {g.Group, Column = c}))
      .Select(c => new
      {
        c.Group,
        c.Column,
        Field = dataFields.FirstOrDefault(f => f.Key == c.Column.Property),
      })
      .Select(c => new DataFieldDto
      {
        Key = c.Column.Property!,
        Category = c.Group!,
        Size = c.Column.Width,
        Description = c.Column.Description ?? string.Empty,
        DisplayName = c.Column.DisplayName ?? c.Field?.DisplayName ?? string.Empty,
        Placeholder = c.Field?.Placeholder ?? string.Empty,
        Readonly = c.Column.Readonly ?? !c.Field?.EditLevels.Contains(level) ?? true,
        FieldType = MapFieldTypeToDto(c.Field?.FieldType),
        Visible = MapVisibilityToDto(c.Column.Visible),
        MaxLength = c.Field?.MaxLength ?? -1,
        SelectableValues = c.Field?.SelectableValues?.ToList() ?? new List<string>(),
        SelectableValuesExt = GetSelectableValuesFromDataField(c.Field!),
        SelectableValuesFactory = c.Field?.SelectableValuesFactory,
      });
    return columnGroups;
  }

  private static List<SelectableValue> GetSelectableValuesFromDataField(DataField dataField)
  {
    var result = new List<SelectableValue>();
    if (dataField?.SelectableValues == null) return result;

    result.AddRange(dataField.SelectableValuesExt.Select(value => new SelectableValue()
      {Value = value.Text, BackgroundColor = value.BackgroundColor, ForegroundColor = value.ForegroundColor,}));

    return result;
  }

  private static ColumnVisibilityDto MapVisibilityToDto(ColumnVisibility visible)
    => visible switch
    {
      ColumnVisibility.Always => ColumnVisibilityDto.Always,
      ColumnVisibility.Expanded => ColumnVisibilityDto.Expanded,
      ColumnVisibility.Collapsed => ColumnVisibilityDto.Collapsed,
      _ => throw new ArgumentOutOfRangeException(nameof(visible), visible, null),
    };

  private static DataFieldTypeDto MapFieldTypeToDto(DataFieldType? fieldType)
    => fieldType switch
    {
      DataFieldType.Text => DataFieldTypeDto.Text,
      DataFieldType.Boolean => DataFieldTypeDto.Boolean,
      DataFieldType.Selection => DataFieldTypeDto.Selection,
      DataFieldType.IpAddress => DataFieldTypeDto.IpAddress,
      null => DataFieldTypeDto.Text,
      _ => throw new ArgumentOutOfRangeException(nameof(fieldType), fieldType, null),
    };
}
