namespace WebIO.DataAccess.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application;
using Entities;
using Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Model;
using Model.Readonly;

public class EfCoreDeviceRepository : IDeviceRepository
{
  private readonly AppDbContext _context;
  private readonly ILogger _log;
  private readonly PseudoProperties _pseudoProperties;

  public EfCoreDeviceRepository(
    AppDbContext context,
    PseudoProperties pseudoProperties,
    ILogger<EfCoreDeviceRepository> log)
  {
    _context = context;
    _pseudoProperties = pseudoProperties;
    _log = log;
  }

  private IIncludableQueryable<DeviceEntity, List<DevicePropertyValueEntity>> Devices =>
    _context.Devices.Include(d => d.Properties);

  private IIncludableQueryable<InterfaceEntity, List<InterfacePropertyValueEntity>> Interfaces =>
    _context.Interfaces.Include(d => d.Properties);

  private IIncludableQueryable<StreamEntity, List<StreamPropertyValueEntity>> Streams =>
    _context.Streams.Include(d => d.Properties);

  public Device GetDevice(Guid deviceId)
  {
    using var span = Telemetry.Span();
    var deviceEntity = Devices.Single(d => d.Id == deviceId);
    var device = deviceEntity.ToModel();

    using (Telemetry.Span())
    {
      var interfaces = Interfaces
        .Where(i => i.DeviceId == device.Id)
        .Select(i => i.ToModel())
        .ToList();

      device.Interfaces.AddRange(interfaces.OrderBy(i => i.Index));

      foreach (var iface in device.Interfaces)
      {
        using (Telemetry.Span())
        {
          iface.Streams.AddRange(Streams
            .Where(s => s.InterfaceId == iface.Id)
            .Select(s => s.ToModel()));
        }
      }
    }

    return device;
  }

  public void Upsert(Device device)
  {
    using var span = Telemetry.Span($"{nameof(UpsertAll)} - {nameof(Device)}");
    // Insert
    if (!_context.Devices.Any(DeviceEntityMapper.DeviceEquals(device)))
    {
      var deviceEntity = device.ToEntity();
      _context.Devices.Add(deviceEntity);
      _context.DevicesDenormalized.Add(device.ToDenormalizedProperties());

      UpsertAll(device.Interfaces.Select(d => (d, device.Id)).ToList());
    }
    else
    {
      var entity = Devices.Single(DeviceEntityMapper.DeviceEquals(device));
      UpdateDeviceInDb(device, entity);
      RemoveDeletedInterfaces(device, entity);
      UpsertRemainingInterfaces(device);
    }

    _context.SaveChanges();
  }

  private void UpdateDeviceInDb(Device device, DeviceEntity entity)
  {
    device.SyncEntity(entity);
    device.UpdateDenormalized(_context.DevicesDenormalized.Find(device.Id)!);
  }

  private void UpsertRemainingInterfaces(Device device)
    => UpsertAll(device.Interfaces.Select(d => (d, device.Id)).ToList());

  private void RemoveDeletedInterfaces(Device device, DeviceEntity entity)
  {
    var deviceInterfaces = Interfaces
      .Where(i => i.DeviceId == entity.Id)
      .ToList();
    var removedInterfaces = deviceInterfaces
      .Where(i => !device.Interfaces.Any(@interface
        => DeviceEntityMapper.InterfaceEquals(@interface, entity.Id).Compile().Invoke(i)));
    _context.Interfaces.RemoveRange(removedInterfaces);
  }

  private IEnumerable<(Interface iface, Guid deviceId)> CreateMissingInterfaces(
    IEnumerable<(Interface iface, Guid deviceId)> ifaces)
  {
    var toCreate = ifaces.Where(s => !_context.Interfaces.Any(DeviceEntityMapper.InterfaceEquals(s.iface, s.deviceId)));
    _context.Interfaces.AddRange(toCreate.Select(t => t.iface.ToEntity(t.deviceId)));
    _context.InterfacesDenormalized.AddRange(toCreate.Select(t => t.iface.ToDenormalizedProperties()));
    return toCreate;
  }

  private void UpsertRemainingStreams(IReadOnlyCollection<(Stream s, Guid Id)> streamList)
    => UpsertAll(streamList);

  private IEnumerable<Guid> UpsertRemainingInterfaces(
    IEnumerable<(Interface iface, Guid deviceId)> ifaces,
    Dictionary<Guid, (Interface iface, Guid deviceId)> toUpdate)
  {
    var ifaceIds = toUpdate.Values.Select(t => t.iface.Id);
    var dbInterfaces = Interfaces.Where(iface => ifaceIds.Contains(iface.Id)).ToList();
    var dbDenormalized = _context.InterfacesDenormalized.Where(iface => ifaceIds.Contains(iface.Id))
      .ToDictionary(iface => iface.Id);

    foreach (var ifaceEntity in dbInterfaces)
    {
      var update = toUpdate[ifaceEntity.Id];
      update.iface.SyncEntity(ifaceEntity);
      update.iface.UpdateDenormalized(dbDenormalized[ifaceEntity.Id]);
    }

    return ifaceIds;
  }

  private void RemoveDeletedStreams(IEnumerable<(Stream s, Guid Id)> streamList, IEnumerable<Guid> ifaceIds)
  {
    var streamListIds = streamList.Select(t => t.s.Id);
    var currentStreams = Streams.Where(s => ifaceIds.Contains(s.InterfaceId)).ToList();
    var toRemove = currentStreams.Where(s => !streamListIds.Contains(s.Id));
    _context.Streams.RemoveRange(toRemove);
  }

  private void UpsertAll(IReadOnlyCollection<(Interface iface, Guid deviceId)> ifaces)
  {
    using var span = Telemetry.Span($"{nameof(UpsertAll)} - {nameof(Interface)}");

    var created = CreateMissingInterfaces(ifaces);
    var toUpdate = ifaces.Except(created).ToList();
    UseDbIds(toUpdate);

    var toUpdateDic = toUpdate.ToDictionary(t => t.iface.Id);
    var ifaceIds = UpsertRemainingInterfaces(ifaces, toUpdateDic);

    var streamList = ifaces.SelectMany(t => t.iface.Streams.Select(s => (s, t.iface.Id))).ToList();
    RemoveDeletedStreams(streamList, ifaceIds);
    UpsertRemainingStreams(streamList);
  }

  private void UseDbIds(List<(Interface iface, Guid deviceId)> toUpdate)
  {
    var deviceIds = toUpdate.Select(t => t.deviceId).Distinct();
    var dbEntities = Interfaces.Where(i => deviceIds.Contains(i.DeviceId)).ToList();

    foreach (var iface in toUpdate)
    {
      var dbEntity = dbEntities.SingleOrDefault(i => i.DeviceId == iface.deviceId && i.Index == iface.iface.Index);
      if (dbEntity != null)
      {
        iface.iface.Id = dbEntity.Id;
      }
    }
  }

  private void UpsertAll(IReadOnlyCollection<(Stream stream, Guid ifaceId)> streams)
  {
    using var span = Telemetry.Span($"{nameof(UpsertAll)} - {nameof(Stream)}");
    var toCreate = streams.Where(s => !_context.Streams.Any(DeviceEntityMapper.StreamEquals(s.stream, s.ifaceId)))
      .ToList();
    var toUpdate = streams.Except(toCreate).ToList();
    UseDbIds(toUpdate);

    var toUpdateDic = toUpdate.ToDictionary(t => t.stream.Id);

    _context.Streams.AddRange(toCreate.Select(t => t.stream.ToEntity(t.ifaceId)));
    var streamIds = toUpdateDic.Values.Select(t => t.stream.Id);

    var dbStreams = Streams.Where(s => streamIds.Contains(s.Id)).ToList();

    foreach (var streamEntity in dbStreams)
    {
      var update = toUpdateDic[streamEntity.Id];
      update.stream.SyncEntity(streamEntity);
    }
  }

  private void UseDbIds(List<(Stream stream, Guid ifaceId)> toUpdate)
  {
    var ifaceIds = toUpdate.Select(t => t.ifaceId).Distinct();
    var dbEntities = Streams.Where(i => ifaceIds.Contains(i.InterfaceId)).ToList();

    foreach (var stream in toUpdate)
    {
      var dbEntity = dbEntities.SingleOrDefault(i => i.InterfaceId == stream.ifaceId && i.Name == stream.stream.Name);
      if (dbEntity != null)
      {
        stream.stream.Id = dbEntity.Id;
      }
    }
  }

  public void InitSchema()
  {
    _log.LogInformation("Applying database schema");
    _context.Database.Migrate();
  }

  public void Delete(Guid deviceId)
  {
    using var span = Telemetry.Span();
    var deviceEntity = Devices.SingleOrDefault(d => d.Id == deviceId);
    if (deviceEntity != null)
    {
      _context.Devices.Remove(deviceEntity);
    }

    var deviceDenormalized = _context.DevicesDenormalized.Find(deviceId);
    if (deviceDenormalized != null)
    {
      _context.DevicesDenormalized.Remove(deviceDenormalized);
    }

    var interfaces = _context.Interfaces.Where(i => i.DeviceId == deviceId);
    _context.RemoveRange(interfaces);
    var interfacesDenormalized = _context.InterfacesDenormalized
      .Join(interfaces, d => d.Id, i => i.Id, (d, i) => d);
    _context.RemoveRange(interfacesDenormalized);

    var streams = _context.Streams
      .Join(interfaces, s => s.InterfaceId, i => i.Id, (s, i) => s);
    _context.RemoveRange(streams);

    _context.SaveChanges();
  }

  public QueryResult<DeviceInfo> GetDeviceInfos(Query query)
  {
    using var span = Telemetry.Span();

    var result = GetDeviceBaseQuery()
      .ApplyFilter(query, _pseudoProperties)
      .ApplySorting(query)
      .Skip(query.StartIndex)
      .Take(query.Count)
      .Select(x => x.Device!.MapToInfo(x.Denormalized))
      .ToImmutableList();

    return new(query.StartIndex, result.Count, result);
  }

  public bool IsDuplicateDeviceName(string deviceName, Guid ownId)
  {
    return GetDeviceBaseQuery()
      .Where(d => d.Device!.Id != ownId)
      .Select(d => d.Device!.Name)
      .Any(dn => string.Equals(dn, deviceName));
  }

  public IEnumerable<Device> GetDevicesByIds(IEnumerable<Guid> deviceIds)
  {
    using var span = Telemetry.Span();
    var deviceIdStrings = deviceIds.ToList();
    var devices = Devices
      .Where(d => deviceIdStrings.Contains(d.Id))
      .Select(d => d.ToModel())
      .ToList();
    var interfaces = Interfaces.Where(i => deviceIdStrings.Contains(i.DeviceId)).ToList().GroupBy(i => i.DeviceId)
      .ToDictionary(g => g.Key, g => g.Select(i => i.ToModel()).ToList());
    var interfaceIds = interfaces.Values.SelectMany(g => g.Select(i => i.Id)).ToList();
    var streams = Streams.Where(s => interfaceIds.Contains(s.InterfaceId)).ToList().GroupBy(s => s.InterfaceId)
      .ToDictionary(g => g.Key, g => g.Select(s => s.ToModel()).ToList());

    foreach (var iface in interfaces.Values.SelectMany(i => i).Where(i => streams.ContainsKey(i.Id)))
    {
      iface.Streams.AddRange(streams[iface.Id]);
    }

    foreach (var device in devices.Where(device => interfaces.ContainsKey(device.Id)))
    {
      device.Interfaces.AddRange(interfaces[device.Id]);
    }

    return devices;
  }

  public int GetDeviceCount(Query query)
  {
    using var span = Telemetry.Span();
    return GetDeviceBaseQuery()
      .ApplyFilter(query, _pseudoProperties)
      .Count();
  }

  private IQueryable<DeviceInfoQueryResult> GetDeviceBaseQuery()
  {
    using var span = Telemetry.Span();
    return _context.Devices
      .AsNoTracking()
      .Include(d => d.Properties)
      .Join(_context.DevicesDenormalized,
        d => d.Id,
        den => den.Id,
        (d, den) => new DeviceInfoQueryResult { Device = d, Denormalized = den });
  }

  public QueryResult<InterfaceInfo> GetInterfaceInfos(Query query)
  {
    using var span = Telemetry.Span();
    var result = GetInterfaceBaseQuery()
      .ApplyFilter(query, _pseudoProperties)
      .ApplySorting(query)
      .Skip(query.StartIndex)
      .Take(query.Count)
      .Select(x => x.Interface.MapToInfo(x.Denormalized, x.Device))
      .ToImmutableList();

    return new(query.StartIndex, result.Count, result);
  }

  public int GetInterfaceCount(Query query)
  {
    using var span = Telemetry.Span();
    return GetInterfaceBaseQuery()
      .ApplyFilter(query, _pseudoProperties)
      .Count();
  }

  private IQueryable<InterfaceInfoQueryResult> GetInterfaceBaseQuery()
  {
    using var span = Telemetry.Span();
    return _context.Interfaces
      .AsNoTracking()
      .Include(x => x.Properties)
      .Join(_context.InterfacesDenormalized,
        i => i.Id,
        den => den.Id,
        (i, den) => new { entity = i, denormalized = den })
      .Join(_context.Devices.Include(x => x.Properties),
        i => i.entity.DeviceId,
        d => d.Id,
        (i, d) => new InterfaceInfoQueryResult
          { Interface = i.entity, Denormalized = i.denormalized, Device = d });
  }

  public QueryResult<StreamInfo> GetStreamInfos(Query query)
  {
    using var span = Telemetry.Span();
    var result = GetStreamBaseQuery()
      .ApplyFilter(query, _pseudoProperties)
      .ApplySorting(query)
      .Skip(query.StartIndex)
      .Take(query.Count)
      .ToList()
      .Select(x => x.Stream.MapToInfo(x.Interface, x.Device))
      .ToImmutableList();

    return new(query.StartIndex, result.Count, result);
  }

  public int GetStreamCount(Query query)
  {
    using var span = Telemetry.Span();
    return GetStreamBaseQuery()
      .ApplyFilter(query, _pseudoProperties)
      .Count();
  }

  private IQueryable<StreamInfoQueryResult> GetStreamBaseQuery()
  {
    return _context.Streams
      .AsNoTracking()
      .Include(x => x.Properties)
      .Join(_context.Interfaces.Include(x => x.Properties),
        s => s.InterfaceId,
        i => i.Id,
        (s, i) => new { stream = s, @interface = i })
      .Join(_context.Devices.Include(x => x.Properties),
        i => i.@interface.DeviceId,
        d => d.Id,
        (s, d) => new StreamInfoQueryResult { Stream = s.stream, Interface = s.@interface, Device = d });
  }

  public async Task<Modification> GetDeviceModificationAsync(Guid deviceId, CancellationToken ct)
    => await _context.Devices
      .AsNoTracking()
      .Where(d => d.Id == deviceId)
      .Select(d => new Modification(
        d.Creator ?? string.Empty,
        d.Created, d.Modifier ?? string.Empty,
        d.Modified,
        d.ModificationComment ?? string.Empty))
      .SingleOrDefaultAsync(ct) ?? Modification.Empty;

  public async Task<Modification> GetInterfaceModificationAsync(Guid interfaceId, CancellationToken ct)
    => await _context.Interfaces
      .AsNoTracking()
      .Where(d => d.Id == interfaceId)
      .Select(d => new Modification(
        d.Creator ?? string.Empty,
        d.Created, d.Modifier ?? string.Empty,
        d.Modified,
        d.ModificationComment ?? string.Empty))
      .SingleOrDefaultAsync(ct) ?? Modification.Empty;

  public async Task<Modification> GetStreamModificationAsync(Guid streamId, CancellationToken ct)
    => await _context.Streams
      .AsNoTracking()
      .Where(d => d.Id == streamId)
      .Select(d => new Modification(
        d.Creator ?? string.Empty,
        d.Created, d.Modifier ?? string.Empty,
        d.Modified,
        d.ModificationComment ?? string.Empty))
      .SingleOrDefaultAsync(ct) ?? Modification.Empty;
}
