namespace WebIO.DataAccess.Elastic;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Nest;
using WebIO.Elastic.Data;
using WebIO.Elastic.Management;
using WebIO.Elastic.Management.IndexManagement;
using WebIO.Elastic.Management.Search;

public class StreamSearcher : Searcher<IndexedStream, StreamSearchRequest, Guid>
{
  public StreamSearcher(
    ILogger<Searcher<IndexedStream, StreamSearchRequest, Guid>> log,
    IElasticClient client,
    IIndexManager<IndexedStream, Guid> indexManager,
    ElasticConfiguration config) : base(log, client, indexManager, config)
  {
  }

  protected override Func<SearchDescriptor<IndexedStream>, SearchDescriptor<IndexedStream>> ToQuery(
    StreamSearchRequest request)
  {
    var mustFilters = new List<Func<QueryContainerDescriptor<IndexedStream>, QueryContainer>>();

    if (!string.IsNullOrWhiteSpace(request.StreamName))
    {
      var fields = new[]
      {
        new TextFieldSelector<IndexedStream>
        {
          Selector = i => i.Name,
          Boost = 5,
          Type = TextFieldType.Exact,
        },
        new TextFieldSelector<IndexedStream>
        {
          Selector = i => i.Name,
          Boost = 2,
          Type = TextFieldType.PrefixWildcard,
        },
      };
      mustFilters.Add(mf => Util.ToTextQuery<IndexedStream, Guid>(mf, request.StreamName, fields));
    }
    

    if (!string.IsNullOrWhiteSpace(request.DeviceName))
    {
      var fields = new[]
      {
        new TextFieldSelector<IndexedStream>
        {
          Selector = i => i.DeviceName,
          Boost = 5,
          Type = TextFieldType.Exact,
        },
        new TextFieldSelector<IndexedStream>
        {
          Selector = i => i.DeviceName,
          Boost = 2,
          Type = TextFieldType.PrefixWildcard,
        },
      };
      mustFilters.Add(mf => Util.ToTextQuery<IndexedStream, Guid>(mf, request.DeviceName, fields));
    }
    
    if (!string.IsNullOrWhiteSpace(request.InterfaceName))
    {
      var fields = new[]
      {
        new TextFieldSelector<IndexedStream>
        {
          Selector = i => i.InterfaceName,
          Boost = 5,
          Type = TextFieldType.Exact,
        },
        new TextFieldSelector<IndexedStream>
        {
          Selector = i => i.InterfaceName,
          Boost = 2,
          Type = TextFieldType.PrefixWildcard,
        },
      };
      mustFilters.Add(mf => Util.ToTextQuery<IndexedStream, Guid>(mf, request.InterfaceName, fields));
    }

    return sd
      => sd
        .MinScore(mustFilters.Any() ? 8 : 0)
        .Query(qd => qd.Bool(bqd =>
      {
        var shouldQueries = request.InterfaceIds.Select(ifaceId
            => (Func<QueryContainerDescriptor<IndexedStream>, QueryContainer>) (sf
              => sf.Match(f => f.Field(str => str.InterfaceId).Query($"{ifaceId}"))))
          .ToArray();

        return bqd.Should(shouldQueries).Must(mustFilters.ToArray());
      }));
  }
}
