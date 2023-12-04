namespace WebIO.Application;

public abstract class ValueGetter
{
  public abstract string GetValue(string property);

  public bool RepresentsTrue(string property)
    => GetValue(property) switch
    {
      "1" => true,
      "yes" => true,
      "ja" => true,
      "x" => true,
      "true" => true,
      "True" => true,
      _ => false,
    };
}
