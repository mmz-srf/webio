namespace WebIO.DataAccess.Elastic;

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Nest;
using WebIO.Elastic.Data;
using WebIO.Elastic.Management;
using WebIO.Elastic.Management.IndexManagement;
using WebIO.Elastic.Management.Search;

public class DeviceSearcher : Searcher<IndexedDevice, DeviceSearchRequest, Guid>
{
  public DeviceSearcher(
    ILogger<Searcher<IndexedDevice, DeviceSearchRequest, Guid>> log,
    IElasticClient client,
    IIndexManager<IndexedDevice, Guid> indexManager,
    ElasticConfiguration config) : base(log, client, indexManager, config)
  {
  }

  protected override Func<SearchDescriptor<IndexedDevice>, SearchDescriptor<IndexedDevice>> ToQuery(
    DeviceSearchRequest request)
  {
    var mustFilters = new List<Func<QueryContainerDescriptor<IndexedDevice>, QueryContainer>>();

    if (!string.IsNullOrWhiteSpace(request.DeviceName))
    {
      mustFilters.Add(mf
        => mf.Match(f
          => f.Field(d => d.Name).Query(request.DeviceName)));
    }

    return sd
      => sd.Query(qd
        => qd.Bool(bqd
          => bqd.Must(mustFilters.ToArray())));
  }
}