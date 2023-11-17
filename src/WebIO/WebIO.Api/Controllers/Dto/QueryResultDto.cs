namespace WebIO.Api.Controllers.Dto;

public record QueryResultDto<T>
{
  public QueryResultState State { get; init; }
  public int Start { get; init; }
  public int Count { get; init; }
  public int TotalCount { get; init; }
  public List<T> Data { get; init; } = new();
}
