namespace WebIO.Elastic.Management.Search;

using Nest;

public record SortFieldDefinition
{
  public string FieldName { get; init; }
  public SortOrder SortOrder { get; init; } = SortOrder.Ascending;
  public string Path { get; init; }
  public string FullName => $"{Path}{FieldName}";
}
