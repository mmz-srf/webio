namespace WebIO.Elastic.Management.Search;

using System.Linq.Expressions;

public record TextFieldSelector<TIndexEntity>
{
  public string Name { get; init; } = string.Empty;
  public Expression<Func<TIndexEntity, object>>? Selector { get; init; }
  public double Boost { get; init; }
  public TextFieldType Type { get; init; }
}
