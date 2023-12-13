namespace WebIO.Elastic.Management.Search;

public record SearchRequest
{
  public int? Take { get; init; }
  public string? Fulltext { get; init; }
  public IEnumerable<SortFieldDefinition> Sorting { get; init; } = Enumerable.Empty<SortFieldDefinition>();
}
