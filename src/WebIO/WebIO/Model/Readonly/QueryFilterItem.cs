namespace WebIO.Model.Readonly;

public class QueryFilterItem
{
  public QueryFilterItem(string key, string value)
  {
    Key = key;
    Value = value;
  }

  public string Key { get; }
  public string Value { get; }

  public override string ToString()
  {
    return $"{Key}: {Value}";
  }
}
