namespace WebIO.Model;

using System.Collections.Generic;
using Extensions;

public class FieldValues
{
  private readonly Dictionary<string, string> _values = new();

  public FieldValues()
  {
  }

  public FieldValues(Dictionary<string, string> values)
  {
    _values = values;
  }

  public string? this[string fieldKey] => _values.ValueOrDefault(fieldKey);

  public string? this[DataField field]
  {
    get => _values.TryGetValue(field.Key, out var result)
      ? result
      : field.DefaultValue;
    set
    {
      _values[field.Key] = value ?? string.Empty;
      if (value == null)
      {
        _values.Remove(field.Key);
      }
    }
  }

  public IDictionary<string, string> All => _values;
}
