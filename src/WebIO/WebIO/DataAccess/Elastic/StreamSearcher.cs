﻿namespace WebIO.DataAccess.Elastic;

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
    if (request.InterfaceIds.Any())
    {
      return FilterByInterfaceIds(request.InterfaceIds);
    }

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

    var propFilters = new List<Func<QueryContainerDescriptor<IndexedStream>, QueryContainer>>();
    foreach (var (key, value) in request.Properties)
    {
      if (!string.IsNullOrWhiteSpace(value))
      {
        var fields = new[]
        {
          new TextFieldSelector<IndexedStream>
          {
            Name = $"properties.{key}",
            Boost = 10,
            Type = TextFieldType.Exact,
          },
          new TextFieldSelector<IndexedStream>
          {
            Name = $"properties.{key}",
            Boost = 8,
            Type = TextFieldType.PrefixWildcard,
          },
        };

        propFilters.Add(mf
          => mf.Nested(nqd
            => nqd
              .Path(d => d.Properties)
              .Query(q => Util.ToTextQuery<IndexedStream, Guid>(q, value, fields))));
      }
    }

    foreach (var (key, value) in request.Properties)
    {
      if (!string.IsNullOrWhiteSpace(value))
      {
        var fields = new[]
        {
          new TextFieldSelector<IndexedStream>
          {
            Name = $"interfaceProperties.{key}",
            Boost = 10,
            Type = TextFieldType.Exact,
          },
          new TextFieldSelector<IndexedStream>
          {
            Name = $"interfaceProperties.{key}",
            Boost = 8,
            Type = TextFieldType.PrefixWildcard,
          },
        };

        propFilters.Add(mf
          => mf.Nested(nqd
            => nqd
              .Path(d => d.InterfaceProperties)
              .Query(q => Util.ToTextQuery<IndexedStream, Guid>(q, value, fields))));
      }
    }

    foreach (var (key, value) in request.Properties)
    {
      if (!string.IsNullOrWhiteSpace(value))
      {
        var fields = new[]
        {
          new TextFieldSelector<IndexedStream>
          {
            Name = $"deviceProperties.{key}",
            Boost = 10,
            Type = TextFieldType.Exact,
          },
          new TextFieldSelector<IndexedStream>
          {
            Name = $"deviceProperties.{key}",
            Boost = 8,
            Type = TextFieldType.PrefixWildcard,
          },
        };

        propFilters.Add(mf
          => mf.Nested(nqd
            => nqd
              .Path(d => d.DeviceProperties)
              .Query(q => Util.ToTextQuery<IndexedStream, Guid>(q, value, fields))));
      }
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

          return bqd.Should(shouldQueries).Must(mustFilters.ToArray()).Should(propFilters);
        }));
  }

  private static Func<SearchDescriptor<IndexedStream>, SearchDescriptor<IndexedStream>> FilterByInterfaceIds(
    IEnumerable<Guid> ifaceIds)
    => sd
      => sd.Query(qd
        => qd.Bool(bqd => bqd.Must(mf
          => mf.Terms(f
            => f.Field(str => str.InterfaceId).Terms(ifaceIds.Select(id => $"{id}"))))));
}
