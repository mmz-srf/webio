namespace WebIO.Model.Readonly;

using System.Collections.Immutable;

public class QueryResult<T>
{
  public QueryResult(int startIndex, int count,ImmutableList<T> data)
  {
    StartIndex = startIndex;
    Count = count;
    //TotalCount = totalCount;
    Data = data;
  }

  public int StartIndex { get; }
  public int Count { get; }
  //public int TotalCount { get; }
  public ImmutableList<T> Data { get; }
}
