namespace WebIO.Elastic.Management.Search;

public record SearchRequest
{
  public int? Take { get; init; }
  public string? Fulltext { get; init; }
  public IEnumerable<KeyValuePair<string, string>> Sorting { get; init; } = Enumerable.Empty<KeyValuePair<string, string>>();
}
