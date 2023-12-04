namespace WebIO.Model;

using System.Collections.Generic;

public class DataField
{
  public DataField()
  {
  }

  public DataField(string key)
  {
    Key = key;
    DisplayName = key;
  }

  public string Key { get; set; } = null!;

  public string DisplayName { get; set; } = null!;

  public string DefaultValue { get; set; } = null!;
  public string Placeholder { get; set; } = null!;
  public int? MaxLength { get; set; }

  public List<FieldLevel> EditLevels { get; } = new();

  public DataFieldType FieldType { get; init; } = DataFieldType.Text;

  public List<string> SelectableValues { get; set; } = new();
  public SelectableValuesFactory SelectableValuesFactory { get; set; } = new();

  public List<DataSelectableValue> SelectableValuesExt { get; set; } = new();
}