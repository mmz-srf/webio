namespace WebIO.Elastic.Management.Search;

using System.Linq.Expressions;

public record TextFieldSelector<TIndexEntity>
{
  public Expression<Func<TIndexEntity, object>> Selector { get; init; } = null!;
  public double Boost { get; init; }
  public TextFieldType Type { get; init; }
}
