namespace WebIO.DataAccess.Elastic;

using System;
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
    return sd
      => sd.Query(qd => qd.Bool(bqd =>
      {
        var shouldQueries = request.InterfaceIds.Select(ifaceId
            => (Func<QueryContainerDescriptor<IndexedStream>, QueryContainer>) (sf
              => sf.Match(f => f.Field(str => str.InterfaceId).Query($"{ifaceId}"))))
          .ToArray();

        return bqd.Should(shouldQueries);
      }));
  }
}
