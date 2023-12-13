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

public class InterfaceSearcher : Searcher<IndexedInterface, InterfaceSearchRequest, Guid>
{
  private readonly IMetadataRepository _metadata;

  public InterfaceSearcher(
    ILogger<Searcher<IndexedInterface, InterfaceSearchRequest, Guid>> log,
    IElasticClient client,
    IIndexManager<IndexedInterface, Guid> indexManager,
    ElasticConfiguration config,
    IMetadataRepository metadata) : base(log, client, indexManager, config)
  {
    _metadata = metadata;
  }

  protected override Func<SearchDescriptor<IndexedInterface>, SearchDescriptor<IndexedInterface>> ToQuery(
    InterfaceSearchRequest request)
  {
    if (request.DeviceId != null && request.DeviceId != Guid.Empty)
    {
      return FilterByDeviceId(request);
    }

    var mustFilters = new List<Func<QueryContainerDescriptor<IndexedInterface>, QueryContainer>>();
    if (!string.IsNullOrWhiteSpace(request.InterfaceName))
    {
      var fields = new[]
      {
        new TextFieldSelector<IndexedInterface>
        {
          Selector = i => i.Name,
          Boost = 10,
          Type = TextFieldType.Exact,
        },
        new TextFieldSelector<IndexedInterface>
        {
          Selector = i => i.Name,
          Boost = 8,
          Type = TextFieldType.PrefixWildcard,
        },
      };
      mustFilters.Add(mf => Util.ToTextQuery<IndexedInterface, Guid>(mf, request.InterfaceName, fields));
    }

    if (!string.IsNullOrWhiteSpace(request.DeviceName))
    {
      var fields = new[]
      {
        new TextFieldSelector<IndexedInterface>
        {
          Selector = i => i.DeviceName,
          Boost = 10,
          Type = TextFieldType.Exact,
        },
        new TextFieldSelector<IndexedInterface>
        {
          Selector = i => i.DeviceName,
          Boost = 8,
          Type = TextFieldType.PrefixWildcard,
        },
      };
      mustFilters.Add(mf => Util.ToTextQuery<IndexedInterface, Guid>(mf, request.DeviceName, fields));
    }

    var propFilters = new List<Func<QueryContainerDescriptor<IndexedInterface>, QueryContainer>>();
    foreach (var (key, value) in request.Properties.Where(kv => _metadata.IsInterfaceProperty(kv.Key)))
    {
      if (!string.IsNullOrWhiteSpace(value))
      {
        var propFields = new[]
        {
          new TextFieldSelector<IndexedInterface>
          {
            Name = $"{nameof(IndexedInterface.InterfaceProperties)}.{key}",
            Boost = 10,
            Type = TextFieldType.Exact,
          },
          new TextFieldSelector<IndexedInterface>
          {
            Name = $"{nameof(IndexedInterface.InterfaceProperties)}.{key}",
            Boost = 8,
            Type = TextFieldType.PrefixWildcard,
          },
        };

        propFilters.Add(mf
          => mf.Nested(nqd
            => nqd
              .Path(d => d.InterfaceProperties)
              .Query(q => Util.ToTextQuery<IndexedInterface, Guid>(q, value, propFields))));
      }
    }

    foreach (var (key, value) in request.Properties.Where(kv => _metadata.IsDeviceProperty(kv.Key)))
    {
      if (!string.IsNullOrWhiteSpace(value))
      {
        var fields = new[]
        {
          new TextFieldSelector<IndexedInterface>
          {
            Name = $"{nameof(IndexedInterface.DeviceProperties)}.{key}",
            Boost = 10,
            Type = TextFieldType.Exact,
          },
          new TextFieldSelector<IndexedInterface>
          {
            Name = $"{nameof(IndexedInterface.DeviceProperties)}.{key}",
            Boost = 8,
            Type = TextFieldType.PrefixWildcard,
          },
        };

        propFilters.Add(mf
          => mf.Nested(nqd
            => nqd
              .Path(d => d.DeviceProperties)
              .Query(q => Util.ToTextQuery<IndexedInterface, Guid>(q, value, fields))));
      }
    }

    return sd
      => sd
        .MinScore(mustFilters.Any() ? 8 : 0)
        .Query(qd
          => qd.Bool(bqd => bqd.Must(mustFilters.ToArray())) &&
             qd.Bool(bqd => bqd.Should(propFilters)));
  }

  private static Func<SearchDescriptor<IndexedInterface>, SearchDescriptor<IndexedInterface>> FilterByDeviceId(
    InterfaceSearchRequest request)
    => sd
      => sd
        .Query(qd
          => qd.Bool(bqd
            => bqd.Must(mf
              => mf.Match(f
                => f.Field(d => d.DeviceId).Query($"{request.DeviceId}")))));
}
