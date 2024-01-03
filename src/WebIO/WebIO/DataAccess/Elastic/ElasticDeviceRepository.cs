namespace WebIO.DataAccess.Elastic;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EntityFrameworkCore;
using Model;
using Model.Readonly;
using WebIO.Elastic.Data;
using WebIO.Elastic.Management.Indexing;
using WebIO.Elastic.Management.Search;
using static Extensions.SearchUtils;
using SortOrder = Nest.SortOrder;

public class ElasticDeviceRepository : IDeviceRepository
{
  private readonly IIndexer<IndexedDevice, Guid> _deviceIndexer;
  private readonly IIndexer<IndexedInterface, Guid> _ifaceIndexer;
  private readonly IIndexer<IndexedStream, Guid> _streamIndexer;

  private readonly ISearcher<IndexedDevice, DeviceSearchRequest, Guid> _deviceSearcher;
  private readonly ISearcher<IndexedInterface, InterfaceSearchRequest, Guid> _ifaceSearcher;
  private readonly ISearcher<IndexedStream, StreamSearchRequest, Guid> _streamSearcher;
  private readonly IMetadataRepository _metadata;

  private readonly EfCoreDeviceRepository _dbRepo;

  public ElasticDeviceRepository(
    IIndexer<IndexedDevice, Guid> deviceIndexer,
    IIndexer<IndexedInterface, Guid> ifaceIndexer,
    IIndexer<IndexedStream, Guid> streamIndexer,
    ISearcher<IndexedDevice, DeviceSearchRequest, Guid> deviceSearcher,
    ISearcher<IndexedInterface, InterfaceSearchRequest, Guid> ifaceSearcher,
    ISearcher<IndexedStream, StreamSearchRequest, Guid> streamSearcher,
    IMetadataRepository metadata,
    EfCoreDeviceRepository dbRepo)
  {
    _deviceIndexer = deviceIndexer;
    _ifaceIndexer = ifaceIndexer;
    _streamIndexer = streamIndexer;
    _deviceSearcher = deviceSearcher;
    _ifaceSearcher = ifaceSearcher;
    _streamSearcher = streamSearcher;
    _metadata = metadata;
    _dbRepo = dbRepo;
  }

  public async Task<Device?> GetDeviceAsync(Guid deviceId, CancellationToken ct)
  {
    var device = await _deviceSearcher.GetAsync(deviceId, ct);
    var ifaces = await _ifaceSearcher.FindAllAsync(new()
    {
      DeviceId = deviceId,
    }, ct);
    var streams = await _streamSearcher.FindAllAsync(new()
    {
      InterfaceIds = ifaces.Documents.ToBlockingEnumerable(cancellationToken: ct).Select(iface => iface.Id),
    }, ct);

    return device == null
      ? null
      : await ToDevice(device, ifaces.Documents.ToBlockingEnumerable(cancellationToken: ct),
        streams.Documents.ToBlockingEnumerable(cancellationToken: ct),
        ct);
  }

  public async Task UpsertAsync(Device device, CancellationToken ct)
  {
    await _deviceIndexer.IndexAsync(ToIndexedDevice(device), ct);
    await _ifaceIndexer.IndexAllAsync(device.Interfaces.Select(iface => ToIndexedInterface(iface, device.Id)), ct);
    await _streamIndexer
      .IndexAllAsync(
        device.Interfaces.SelectMany(iface
          => iface.Streams.Select(stream => ToIndexedStream(stream, iface.Id))), ct);
  }

  public async Task DeleteAsync(Guid deviceId, CancellationToken ct)
  {
    var device = await GetDeviceAsync(deviceId, ct);

    if (device != null)
    {
      await _streamIndexer.DeleteAllAsync(
        device.Interfaces.SelectMany(iface => iface.Streams.Select(stream => stream.Id)), ct);
      await _ifaceIndexer.DeleteAllAsync(device.Interfaces.Select(iface => iface.Id), ct);
      await _deviceIndexer.DeleteAsync(deviceId, ct);
    }
  }

  public async Task<QueryResult<DeviceInfo>> GetDeviceInfosAsync(Query query, CancellationToken ct)
  {
    var result = await FindDeviceByQueryAsync(query, ct);
    return new(
      query.StartIndex,
      query.Count,
      (await result.Documents.Select(ToDeviceInfo).ToListAsync(ct)).ToImmutableList());
  }

  public async Task<int> GetDeviceCountAsync(Query query, CancellationToken ct)
    => (int) (await FindDeviceByQueryAsync(query, ct)).Total;

  public async Task<QueryResult<InterfaceInfo>> GetInterfaceInfosAsync(Query query, CancellationToken ct)
  {
    var result = await FindInterfaceByQueryAsync(query, ct);
    return new(
      query.StartIndex,
      query.Count,
      (await result.Documents.Select(ToInterfaceInfo).ToListAsync(ct)).ToImmutableList());
  }

  public async Task<int> GetInterfaceCountAsync(Query query, CancellationToken ct)
    => (int) (await FindInterfaceByQueryAsync(query, ct)).Total;

  public async Task<QueryResult<StreamInfo>> GetStreamInfosAsync(Query query, CancellationToken ct)
  {
    var result = await FindStreamByQueryAsync(query, ct);
    return new(
      query.StartIndex,
      query.Count,
      (await result.Documents.Select(ToStreamInfo).ToListAsync(ct)).ToImmutableList());
  }

  public async Task<int> GetStreamCountAsync(Query query, CancellationToken ct)
    => (int) (await FindStreamByQueryAsync(query, ct)).Total;

  public async Task<bool> IsDuplicateDeviceNameAsync(string deviceName, Guid ownId, CancellationToken ct)
  {
    var existingDevices = await _deviceSearcher.FindAllAsync(new()
    {
      DeviceName = deviceName,
    }, ct);

    var existingDevice = await existingDevices.Documents.SingleOrDefaultAsync(cancellationToken: ct);
    return existingDevice != null && existingDevice.Id != ownId;
  }

  public async Task<IEnumerable<Device>> GetDevicesByIdsAsync(IEnumerable<Guid> deviceIds, CancellationToken ct)
    => (await _deviceSearcher.GetAllAsync(deviceIds, ct)).Select(idev
      => ToDevice(idev, new List<IndexedInterface>(), new List<IndexedStream>(), ct).GetAwaiter().GetResult());  // TODO: use async

  private Task<SearchResult<IndexedDevice>> FindDeviceByQueryAsync(Query query, CancellationToken ct)
    => _deviceSearcher.FindAllAsync(new()
    {
      Take = query.Count,
      Fulltext = query.GlobalFilter,
      DeviceName = query.Filter.FirstOrDefault(f => f.Key == "Name")?.Value ??
                   string.Empty,
      Properties = query.Filter.ToDictionary(i => i.Key, i => i.Value),
      Sorting = ToSortDictionary(query),
    }, ct);

  private Task<SearchResult<IndexedInterface>> FindInterfaceByQueryAsync(Query query, CancellationToken ct)
  {
    var deviceId = query.Filter.FirstOrDefault(f => f.Key == nameof(InterfaceSearchRequest.DeviceId))?.Value;
    var deviceGuid = Guid.TryParse(deviceId, out var guid) ? guid : Guid.Empty;
    return _ifaceSearcher.FindAllAsync(new()
    {
      Take = query.Count,
      Fulltext = query.GlobalFilter,
      DeviceId = deviceGuid,
      InterfaceName = query.Filter.FirstOrDefault(f => f.Key == "Name")?.Value ??
                      string.Empty,
      DeviceName = query.Filter.FirstOrDefault(f => f.Key == nameof(InterfaceSearchRequest.DeviceName))?.Value ??
                   string.Empty,
      Sorting = ToSortDictionary(query),
      Properties = query.Filter.ToDictionary(i => i.Key, i => i.Value),
    }, ct);
  }

  private Task<SearchResult<IndexedStream>> FindStreamByQueryAsync(Query query, CancellationToken ct)
    => _streamSearcher.FindAllAsync(new()
    {
      Take = query.Count,
      Fulltext = query.GlobalFilter,
      StreamName = query.Filter.FirstOrDefault(f => f.Key == "Name")?.Value ??
                   string.Empty,
      InterfaceName = query.Filter.FirstOrDefault(f => f.Key == nameof(StreamSearchRequest.InterfaceName))?.Value ??
                      string.Empty,
      DeviceName = query.Filter.FirstOrDefault(f => f.Key == nameof(StreamSearchRequest.DeviceName))?.Value ??
                   string.Empty,
      Sorting = ToSortDictionary(query),
      Properties = query.Filter.ToDictionary(i => i.Key, i => i.Value),
    }, ct);

  private async Task<Device> ToDevice(
    IndexedDevice device,
    IEnumerable<IndexedInterface> interfaces,
    IEnumerable<IndexedStream> streams,
    CancellationToken ct)
    => new()
    {
      Id = device.Id,
      Name = device.Name,
      DeviceType = device.DeviceType,
      Comment = device.Comment,
      Properties = new(new(device.DeviceProperties)),
      Interfaces = interfaces.Select(iface => ToInterface(iface, streams)).ToList(),
      Modification = await _dbRepo.GetDeviceModificationAsync(device.Id, ct),
    };

  private Interface ToInterface(
    IndexedInterface iface,
    IEnumerable<IndexedStream> streams)
    => new()
    {
      Id = iface.Id,
      Name = iface.Name,
      Index = iface.Index,
      InterfaceTemplate = iface.InterfaceTemplate,
      Comment = iface.Comment,
      Streams = streams.Where(stream => stream.InterfaceId == iface.Id).Select(ToStream).ToList(),
      Modification = _dbRepo.GetInterfaceModificationAsync(iface.Id, default).GetAwaiter().GetResult(), // todo: use async
    };

  private Stream ToStream(IndexedStream stream)
    => new()
    {
      Id = stream.Id,
      Name = stream.Name,
      Comment = stream.Comment,
      Type = stream.Type,
      Direction = stream.Direction,
      Modification = _dbRepo.GetStreamModificationAsync(stream.Id, default).GetAwaiter().GetResult(), // todo: use async
    };

  private static IndexedDevice ToIndexedDevice(Device device)
    => new()
    {
      Id = device.Id,
      Name = device.Name,
      DeviceType = device.DeviceType,
      Comment = device.Comment,
      DeviceProperties = device.Properties.All.ToImmutableDictionary(),
      InterfaceCount = device.Interfaces.Count,
    };

  private static IndexedInterface ToIndexedInterface(Interface iface, Guid deviceId)
    => new()
    {
      Id = iface.Id,
      DeviceId = deviceId,
      Name = iface.Name,
      Index = iface.Index,
      InterfaceTemplate = iface.InterfaceTemplate ?? string.Empty,
      Comment = iface.Comment,
      InterfaceProperties = iface.Properties.All.ToImmutableDictionary(),
      StreamsCountVideoSend =
        iface.Streams.Count(s => s is { Type: StreamType.Video, Direction: StreamDirection.Send }),
      StreamsCountAudioSend =
        iface.Streams.Count(s => s is { Type: StreamType.Audio, Direction: StreamDirection.Send }),
      StreamsCountAncillarySend =
        iface.Streams.Count(s => s is { Type: StreamType.Ancillary, Direction: StreamDirection.Send }),
      StreamsCountVideoReceive =
        iface.Streams.Count(s => s is { Type: StreamType.Video, Direction: StreamDirection.Receive }),
      StreamsCountAudioReceive =
        iface.Streams.Count(s => s is { Type: StreamType.Audio, Direction: StreamDirection.Receive }),
      StreamsCountAncillaryReceive =
        iface.Streams.Count(s => s is { Type: StreamType.Ancillary, Direction: StreamDirection.Receive }),
    };

  private static IndexedStream ToIndexedStream(Stream stream, Guid ifaceId)
    => new()
    {
      Id = stream.Id,
      InterfaceId = ifaceId,
      Name = stream.Name,
      Comment = stream.Comment,
      Type = stream.Type,
      Direction = stream.Direction,
      StreamProperties = stream.Properties.All.ToImmutableDictionary(),
    };

  private static DeviceInfo ToDeviceInfo(IndexedDevice device)
    => new(
      device.Id,
      device.Name,
      device.DeviceType,
      device.Comment,
      device.DeviceProperties.ToImmutableDictionary(),
      // new ModificationInfo(), 
      device.InterfaceCount);

  private static InterfaceInfo ToInterfaceInfo(IndexedInterface iface)
    => new(
      iface.Id,
      iface.Name,
      iface.Comment,
      iface.DeviceId,
      iface.DeviceType,
      iface.DeviceName,
      iface.InterfaceTemplate,
      new(
        iface.StreamsCountVideoSend,
        iface.StreamsCountAudioSend,
        iface.StreamsCountAncillarySend,
        iface.StreamsCountVideoReceive,
        iface.StreamsCountAudioReceive,
        iface.StreamsCountAncillaryReceive),
      iface.InterfaceProperties.ToImmutableDictionary(),
      iface.DeviceProperties.ToImmutableDictionary(),
      null
    );

  private static StreamInfo ToStreamInfo(IndexedStream stream)
    => new(
      stream.Id,
      stream.Name,
      stream.Comment,
      stream.Type,
      stream.Direction,
      stream.DeviceId,
      stream.DeviceType,
      stream.DeviceName,
      stream.InterfaceName,
      stream.StreamProperties.ToImmutableDictionary(),
      stream.DeviceProperties.ToImmutableDictionary(),
      stream.InterfaceProperties.ToImmutableDictionary(),
      null);

  private IEnumerable<SortFieldDefinition> ToSortDictionary(Query query)
    => query.Sort?.Split(',')
         .Zip(query.Order?.Split(',') ?? Enumerable.Empty<string>(), (key, value) => new SortFieldDefinition()
         {
           FieldName = key,
           SortOrder = value == "asc" ? SortOrder.Ascending : SortOrder.Descending,
           Path = GetPathForField(key),
         })
       ?? ImmutableArray<SortFieldDefinition>.Empty;

  private string GetPathForField(string field)
  {
    if (IsBaseTypeField(field))
    {
      return string.Empty;
    }

    if (_metadata.IsDeviceProperty(field))
    {
      return "DeviceProperties.";
    }

    if (_metadata.IsInterfaceProperty(field))
    {
      return "InterfaceProperties.";
    }

    if (_metadata.IsStreamProperty(field))
    {
      return "StreamProperties.";
    }

    return string.Empty;
  }
}
