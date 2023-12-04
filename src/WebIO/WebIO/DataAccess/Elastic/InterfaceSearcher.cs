namespace WebIO.DataAccess.Elastic;

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Nest;
using WebIO.Elastic.Data;
using WebIO.Elastic.Management;
using WebIO.Elastic.Management.IndexManagement;
using WebIO.Elastic.Management.Search;

public class InterfaceSearcher : Searcher<IndexedInterface, InterfaceSearchRequest, Guid>
{
  public InterfaceSearcher(
    ILogger<Searcher<IndexedInterface, InterfaceSearchRequest, Guid>> log,
    IElasticClient client,
    IIndexManager<IndexedInterface, Guid> indexManager,
    ElasticConfiguration config) : base(log, client, indexManager, config)
  {
  }

  protected override Func<SearchDescriptor<IndexedInterface>, SearchDescriptor<IndexedInterface>> ToQuery(
    InterfaceSearchRequest request)
  {
    var mustFilters = new List<Func<QueryContainerDescriptor<IndexedInterface>, QueryContainer>>();

    if (request.DeviceId != null && request.DeviceId != Guid.Empty)
    {
      mustFilters.Add(mf
        => mf.Match(f
          => f.Field(d => d.DeviceId).Query($"{request.DeviceId}")));
    }
    
    if (!string.IsNullOrWhiteSpace(request.InterfaceName))
    {
      mustFilters.Add(mf
        => mf.Match(f
          => f.Field(d => d.Name).Query($"{request.InterfaceName}")));
    }

    if (!string.IsNullOrWhiteSpace(request.InterfaceName))
    {
      mustFilters.Add(mf
        => mf.Match(f
          => f.Field(d => d.DeviceName).Query($"{request.DeviceName}")));
    }

    return sd
      => sd.Query(qd
        => qd.Bool(bqd
          => bqd.Must(mustFilters.ToArray())));
  }
}
