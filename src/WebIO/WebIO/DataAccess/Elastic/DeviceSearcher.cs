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

public class DeviceSearcher : Searcher<IndexedDevice, DeviceSearchRequest, Guid>
{
  private readonly IMetadataRepository _metadata;

  public DeviceSearcher(
    ILogger<Searcher<IndexedDevice, DeviceSearchRequest, Guid>> log,
    IElasticClient client,
    IIndexManager<IndexedDevice, Guid> indexManager,
    ElasticConfiguration config,
    IMetadataRepository metadata) : base(log, client, indexManager, config)
  {
    _metadata = metadata;
  }

  protected override Func<SearchDescriptor<IndexedDevice>, SearchDescriptor<IndexedDevice>> ToQuery(
    DeviceSearchRequest request)
  {
    var mustFilters = new List<Func<QueryContainerDescriptor<IndexedDevice>, QueryContainer>>();

    if (!string.IsNullOrWhiteSpace(request.DeviceName))
    {
      var fields = new[]
      {
        new TextFieldSelector<IndexedDevice>
        {
          Selector = i => i.Name,
          Boost = 10,
          Type = TextFieldType.Exact,
        },
        new TextFieldSelector<IndexedDevice>
        {
          Selector = i => i.Name,
          Boost = 8,
          Type = TextFieldType.PrefixWildcard,
        },
      };
      mustFilters.Add(mf => Util.ToTextQuery<IndexedDevice, Guid>(mf, request.DeviceName, fields));
    }

    foreach (var (key, value) in request.Properties.Where(kv=> _metadata.IsDeviceProperty(kv.Key)))
    {
      if (!string.IsNullOrWhiteSpace(value))
      {
        var fields = new[]
        {
          new TextFieldSelector<IndexedDevice>
          {
            Name = $"{nameof(IndexedDevice.DeviceProperties)}.{key}",
            Boost = 10,
            Type = TextFieldType.Exact,
          },
          new TextFieldSelector<IndexedDevice>
          {
            Name = $"{nameof(IndexedDevice.DeviceProperties)}.{key}",
            Boost = 8,
            Type = TextFieldType.PrefixWildcard,
          },
        };

        mustFilters.Add(mf
          => mf.Nested(nqd
            => nqd
              .Path(d => d.DeviceProperties)
              .Query(q => Util.ToTextQuery<IndexedDevice, Guid>(q, value, fields))));
      }
    }

    return sd
      => sd
        .MinScore(mustFilters.Any() ? 8 : 0)
        .Query(qd
          => qd.Bool(bqd
            => bqd.Must(mustFilters.ToArray())));
  }
}
