namespace WebIO.Elastic.Management.Search;

public record SearchRequest
{
  public int? Take { get; init; }
  public string? Fulltext { get; init; }
}
