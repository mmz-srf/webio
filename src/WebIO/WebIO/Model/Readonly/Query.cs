namespace WebIO.Model.Readonly;

using System.Collections.Generic;
using System.Linq;

public class Query
{
  public Query(int startIndex, int count)
  {
    StartIndex = startIndex;
    Count = count;
    Filter = new List<QueryFilterItem>();
  }

  public Query(int startIndex, int count, string? sort, string? order, IList<QueryFilterItem>? filter = null, string? globalFilter = null)
  {
    StartIndex = startIndex;
    Count = count;
    Sort = sort;
    Order = order;
    GlobalFilter = globalFilter;
    Filter = filter ?? new List<QueryFilterItem>();
  }

  public int StartIndex { get; }

  public int Count { get; }

  public string? Sort { get; }

  public string? Order { get; }

  public string? GlobalFilter { get; }

  public IList<QueryFilterItem> Filter { get; }

  public Query WithSorting(string? sort, string? order)
  {
    return new(StartIndex, Count, sort, order, Filter, GlobalFilter);
  }

  public Query WithFilter(Dictionary<string, string> filter, string? globalFilter)
  {
    var filterItems = filter
      .Select(item => new QueryFilterItem(item.Key, item.Value))
      .ToList();

    return new(StartIndex, Count, Sort, Order, filterItems, globalFilter);
  }
}
