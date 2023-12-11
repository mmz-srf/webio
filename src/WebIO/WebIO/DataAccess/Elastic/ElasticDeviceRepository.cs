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

public class ElasticDeviceRepository : IDeviceRepository
{
  private readonly IIndexer<IndexedDevice, Guid> _deviceIndexer;
  private readonly IIndexer<IndexedInterface, Guid> _ifaceIndexer;
  private readonly IIndexer<IndexedStream, Guid> _streamIndexer;

  private readonly ISearcher<IndexedDevice, DeviceSearchRequest, Guid> _deviceSearcher;
  private readonly ISearcher<IndexedInterface, InterfaceSearchRequest, Guid> _ifaceSearcher;
  private readonly ISearcher<IndexedStream, StreamSearchRequest, Guid> _streamSearcher;

  private readonly EfCoreDeviceRepository _dbRepo;

  public ElasticDeviceRepository(
    IIndexer<IndexedDevice, Guid> deviceIndexer,
    IIndexer<IndexedInterface, Guid> ifaceIndexer,
    IIndexer<IndexedStream, Guid> streamIndexer,
    ISearcher<IndexedDevice, DeviceSearchRequest, Guid> deviceSearcher,
    ISearcher<IndexedInterface, InterfaceSearchRequest, Guid> ifaceSearcher,
    ISearcher<IndexedStream, StreamSearchRequest, Guid> streamSearcher,
    EfCoreDeviceRepository dbRepo)
  {
    _deviceIndexer = deviceIndexer;
    _ifaceIndexer = ifaceIndexer;
    _streamIndexer = streamIndexer;
    _deviceSearcher = deviceSearcher;
    _ifaceSearcher = ifaceSearcher;
    _streamSearcher = streamSearcher;
    _dbRepo = dbRepo;
  }

  public Device? GetDevice(Guid deviceId)
  {
    var device = _deviceSearcher.GetAsync(deviceId, default).GetAwaiter().GetResult(); // todo use async
    var ifaces = _ifaceSearcher.FindAllAsync(new()
    {
      DeviceId = deviceId,
    }, default).GetAwaiter().GetResult(); // todo use async
    var streams = _streamSearcher.FindAllAsync(new()
    {
      InterfaceIds = ifaces.Documents.ToBlockingEnumerable().Select(iface => iface.Id),
    }, default).GetAwaiter().GetResult();

    return device == null
      ? null
      : ToDevice(device, ifaces.Documents.ToBlockingEnumerable(), streams.Documents.ToBlockingEnumerable(), default)
        .GetAwaiter().GetResult(); // todo use async
  }

  public void Upsert(Device device)
  {
    _deviceIndexer.IndexAsync(ToIndexedDevice(device), default).GetAwaiter().GetResult(); // todo: use async
    _ifaceIndexer.IndexAllAsync(device.Interfaces.Select(iface => ToIndexedInterface(iface, device.Id)), default)
      .GetAwaiter().GetResult(); // todo: use async
    _streamIndexer
      .IndexAllAsync(
        device.Interfaces.SelectMany(iface
          => iface.Streams.Select(stream => ToIndexedStream(stream, iface.Id))), default)
      .GetAwaiter().GetResult(); // todo: use async
  }

  public void Delete(Guid deviceId)
  {
    var device = GetDevice(deviceId);

    if (device != null)
    {
      _streamIndexer.DeleteAllAsync(device.Interfaces.SelectMany(iface => iface.Streams.Select(stream => stream.Id)),
        default).GetAwaiter().GetResult(); // todo: use async
      _ifaceIndexer.DeleteAllAsync(device.Interfaces.Select(iface => iface.Id), default).GetAwaiter()
        .GetResult(); // todo: use async
      _deviceIndexer.DeleteAsync(deviceId, default).GetAwaiter().GetResult(); // todo: use async
    }
  }

  public QueryResult<DeviceInfo> GetDeviceInfos(Query query)
  {
    var result = FindDeviceByQuery(query);
    return new(
      query.StartIndex,
      query.Count,
      result.Documents.Select(ToDeviceInfo).ToBlockingEnumerable().ToImmutableList());
  }

  public int GetDeviceCount(Query query)
    => (int) FindDeviceByQuery(query).Total;

  public QueryResult<InterfaceInfo> GetInterfaceInfos(Query query)
  {
    var result = FindInterfaceByQuery(query);
    return new(
      query.StartIndex,
      query.Count,
      result.Documents.Select(ToInterfaceInfo).ToBlockingEnumerable().ToImmutableList());
  }

  public int GetInterfaceCount(Query query)
    => (int) FindInterfaceByQuery(query).Total;

  public QueryResult<StreamInfo> GetStreamInfos(Query query)
  {
    var result = FindStreamByQuery(query);
    return new(
      query.StartIndex,
      query.Count,
      result.Documents.Select(ToStreamInfo).ToBlockingEnumerable().ToImmutableList());
  }

  public int GetStreamCount(Query query)
    => (int) FindStreamByQuery(query).Total;

  public bool IsDuplicateDeviceName(string deviceName, Guid ownId)
  {
    var existingDevices = _deviceSearcher.FindAllAsync(new()
    {
      DeviceName = deviceName,
    }, default).GetAwaiter().GetResult(); // todo use async

    var existingDevice = existingDevices.Documents.SingleOrDefaultAsync().GetAwaiter().GetResult();
    return existingDevice != null && existingDevice.Id != ownId;
  }

  public IEnumerable<Device> GetDevicesByIds(IEnumerable<Guid> deviceIds)
    => _deviceSearcher.GetAllAsync(deviceIds, default).GetAwaiter().GetResult().Select(idev
      => ToDevice(idev, new List<IndexedInterface>(), new List<IndexedStream>(), default).GetAwaiter()
        .GetResult()); // todo use async

  private SearchResult<IndexedDevice> FindDeviceByQuery(Query query)
    => _deviceSearcher.FindAllAsync(new()
    {
      Take = query.Count,
      Fulltext = query.GlobalFilter,
      DeviceName = query.Filter.FirstOrDefault(f => f.Key == "Name")?.Value ??
                   string.Empty,
      Properties = query.Filter.ToDictionary(i => i.Key, i => i.Value),
      Sorting = ToSortDictionary(query),
    }, default).GetAwaiter().GetResult();

  private SearchResult<IndexedInterface> FindInterfaceByQuery(Query query)
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
    }, default).GetAwaiter().GetResult();
  }

  private SearchResult<IndexedStream> FindStreamByQuery(Query query)
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
    }, default).GetAwaiter().GetResult();

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
      Properties = new(new(device.Properties)),
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
      Modification = _dbRepo.GetInterfaceModificationAsync(iface.Id, default).GetAwaiter().GetResult(),
    };

  private Stream ToStream(IndexedStream stream)
    => new()
    {
      Id = stream.Id,
      Name = stream.Name,
      Comment = stream.Comment,
      Type = stream.Type,
      Direction = stream.Direction,
      Modification = _dbRepo.GetStreamModificationAsync(stream.Id, default).GetAwaiter().GetResult(),
    };

  private static IndexedDevice ToIndexedDevice(Device device)
    => new()
    {
      Id = device.Id,
      Name = device.Name,
      DeviceType = device.DeviceType,
      Comment = device.Comment,
      Properties = device.Properties.All.ToImmutableDictionary(),
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
      Properties = iface.Properties.All.ToImmutableDictionary(),
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
      Properties = stream.Properties.All.ToImmutableDictionary(),
    };

  private static DeviceInfo ToDeviceInfo(IndexedDevice device)
    => new(
      device.Id,
      device.Name,
      device.DeviceType,
      device.Comment,
      device.Properties.ToImmutableDictionary(),
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
      iface.Properties.ToImmutableDictionary(),
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
      stream.Properties.ToImmutableDictionary(),
      stream.DeviceProperties.ToImmutableDictionary(),
      stream.InterfaceProperties.ToImmutableDictionary(),
      null);

  private static IEnumerable<KeyValuePair<string, string>> ToSortDictionary(Query query)
    => query.Sort?.Split(',').Select(LcFirstIfBaseObjectMember)
      .Zip(query.Order?.Split(',') ?? Enumerable.Empty<string>(), KeyValuePair.Create) 
       ?? ImmutableArray<KeyValuePair<string, string>>.Empty;

  private static string LcFirstIfBaseObjectMember(string field)
    => field switch {
      "Name" => "name",
      "DeviceName" => "deviceName",
      "InterfaceName" => "interfaceName",
      "StreamName" => "streamName",
      _ => $"properties.{field}",
    };
}

public record DeviceSearchRequest : SearchRequest
{
  public string DeviceName { get; init; } = string.Empty;
  public IReadOnlyDictionary<string, string> Properties { get; init; } = ImmutableDictionary<string, string>.Empty;
}

public record InterfaceSearchRequest : SearchRequest
{
  public Guid? DeviceId { get; init; }
  public string InterfaceName { get; init; } = string.Empty;
  public string DeviceName { get; init; } = string.Empty;
  public IReadOnlyDictionary<string, string> Properties { get; init; } = ImmutableDictionary<string, string>.Empty;
}

public record StreamSearchRequest : SearchRequest
{
  public string DeviceName { get; init; } = string.Empty;
  public string InterfaceName { get; init; } = string.Empty;
  public IEnumerable<Guid> InterfaceIds { get; init; } = new List<Guid>();
  public string StreamName { get; init; } = string.Empty;
  public IReadOnlyDictionary<string, string> Properties { get; init; } = ImmutableDictionary<string, string>.Empty;
}
