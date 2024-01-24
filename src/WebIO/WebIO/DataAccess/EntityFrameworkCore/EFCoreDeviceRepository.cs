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

  public async Task<Device?> GetDeviceAsync(Guid deviceId, CancellationToken ct)
  {
    using var span = Telemetry.Span();
    var deviceEntity = await Devices.SingleAsync(d => d.Id == deviceId, ct);
    var device = deviceEntity.ToModel();

    using (Telemetry.Span())
    {
      var interfaces = await Interfaces
        .Where(i => i.DeviceId == device.Id)
        .Select(i => i.ToModel())
        .ToListAsync(ct);

      device.Interfaces.AddRange(interfaces.OrderBy(i => i.Index));

      foreach (var iface in device.Interfaces)
      {
        using (Telemetry.Span())
        {
          iface.Streams.AddRange(await Streams
            .Where(s => s.InterfaceId == iface.Id)
            .Select(s => s.ToModel())
            .ToListAsync(ct));
        }
      }
    }

    return device;
  }

  public async Task UpsertAsync(Device device, CancellationToken ct)
  {
    using var span = Telemetry.Span($"{nameof(UpsertAllAsync)} - {nameof(Device)}");
    // Insert
    if (!await _context.Devices.AnyAsync(DeviceEntityMapper.DeviceEquals(device), ct))
    {
      var deviceEntity = device.ToEntity();
      _context.Devices.Add(deviceEntity);
      _context.DevicesDenormalized.Add(device.ToDenormalizedProperties());

      await UpsertAllAsync(device.Interfaces.Select(d => (d, device.Id)).ToList(), ct);
    }
    else
    {
      var entity = await Devices.SingleAsync(DeviceEntityMapper.DeviceEquals(device), ct);
      await UpdateDeviceInDb(device, entity, ct);
      RemoveDeletedInterfaces(device, entity);
      await UpsertRemainingInterfacesAsync(device, ct);
    }

    await _context.SaveChangesAsync(ct);
  }

  private async Task UpdateDeviceInDb(Device device, DeviceEntity entity, CancellationToken? ct)
  {
    device.SyncEntity(entity);
    device.UpdateDenormalized(await _context.DevicesDenormalized.SingleAsync(dd => dd.Id == device.Id));
  }

  private Task UpsertRemainingInterfacesAsync(Device device, CancellationToken ct)
    => UpsertAllAsync(device.Interfaces.Select(d => (d, device.Id)).ToList(), ct);

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
    var toCreate = ifaces.Where(s => !_context.Interfaces.Any(DeviceEntityMapper.InterfaceEquals(s.iface, s.deviceId))).ToList();
    _context.Interfaces.AddRange(toCreate.Select(t => t.iface.ToEntity(t.deviceId)));
    _context.InterfacesDenormalized.AddRange(toCreate.Select(t => t.iface.ToDenormalizedProperties()));
    return toCreate;
  }

  private void UpsertRemainingStreams(IReadOnlyCollection<(Stream s, Guid Id)> streamList)
    => UpsertAllAsync(streamList);

  private async Task<IEnumerable<Guid>> UpsertRemainingInterfacesAsync(
    IEnumerable<(Interface iface, Guid deviceId)> ifaces,
    Dictionary<Guid, (Interface iface, Guid deviceId)> toUpdate,
    CancellationToken ct)
  {
    var ifaceIds = toUpdate.Values.Select(t => t.iface.Id);
    var dbInterfaces = await Interfaces.Where(iface => ifaceIds.Contains(iface.Id)).ToListAsync(ct);
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

  private async Task UpsertAllAsync(IReadOnlyCollection<(Interface iface, Guid deviceId)> ifaces, CancellationToken ct)
  {
    using var span = Telemetry.Span($"{nameof(UpsertAllAsync)} - {nameof(Interface)}");

    var created = CreateMissingInterfaces(ifaces);
    var toUpdate = ifaces.Except(created).ToList();
    await UseDbIdsAsync(toUpdate, ct);

    var toUpdateDic = toUpdate.ToDictionary(t => t.iface.Id);
    var ifaceIds = await UpsertRemainingInterfacesAsync(ifaces, toUpdateDic, ct);

    var streamList = ifaces.SelectMany(t => t.iface.Streams.Select(s => (s, t.iface.Id))).ToList();
    RemoveDeletedStreams(streamList, ifaceIds);
    UpsertRemainingStreams(streamList);
  }

  private async Task UseDbIdsAsync(List<(Interface iface, Guid deviceId)> toUpdate, CancellationToken ct)
  {
    var deviceIds = toUpdate.Select(t => t.deviceId).Distinct();
    var dbEntities = await Interfaces.Where(i => deviceIds.Contains(i.DeviceId)).ToListAsync(cancellationToken: ct);

    foreach (var iface in toUpdate)
    {
      var dbEntity = dbEntities.SingleOrDefault(i => i.DeviceId == iface.deviceId && i.Index == iface.iface.Index);
      if (dbEntity != null)
      {
        iface.iface.Id = dbEntity.Id;
      }
    }
  }

  private void UpsertAllAsync(IReadOnlyCollection<(Stream stream, Guid ifaceId)> streams)
  {
    using var span = Telemetry.Span($"{nameof(UpsertAllAsync)} - {nameof(Stream)}");
    var toCreate = streams.Where(s => !_context.Streams.Any(DeviceEntityMapper.StreamEquals(s.stream, s.ifaceId)))
      .ToList();
    var toUpdate = streams.Except(toCreate).ToList();
    UseDbIdsAsync(toUpdate);

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

  private void UseDbIdsAsync(List<(Stream stream, Guid ifaceId)> toUpdate)
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

  public Task InitSchema(CancellationToken ct)
  {
    _log.LogInformation("Applying database schema");
    return _context.Database.MigrateAsync(ct);
  }

  public async Task DeleteAsync(Guid deviceId, CancellationToken ct)
  {
    using var span = Telemetry.Span();
    var deviceEntity = Devices.SingleOrDefault(d => d.Id == deviceId);
    if (deviceEntity != null)
    {
      _context.Devices.Remove(deviceEntity);
    }

    var deviceDenormalized = await _context.DevicesDenormalized.FindAsync(new object?[] { deviceId }, cancellationToken: ct);
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

    await _context.SaveChangesAsync(ct);
  }

  public async Task<QueryResult<DeviceInfo>> GetDeviceInfosAsync(Query query, CancellationToken ct)
  {
    using var span = Telemetry.Span();

    var result = (await GetDeviceBaseQuery()
        .ApplyFilter(query, _pseudoProperties)
        .ApplySorting(query)
        .Skip(query.StartIndex)
        .Take(query.Count)
        .Select(x => x.Device.MapToInfo(x.Denormalized))
        .ToListAsync(ct))
      .ToImmutableList();

    return new(query.StartIndex, result.Count, result);
  }

  public Task<bool> IsDuplicateDeviceNameAsync(string deviceName, Guid ownId, CancellationToken ct)
    => GetDeviceBaseQuery()
      .Where(d => d.Device.Id != ownId)
      .Select(d => d.Device.Name)
      .AnyAsync(dn => string.Equals(dn, deviceName), cancellationToken: ct);

  public async Task<IEnumerable<Device>> GetDevicesByIdsAsync(IEnumerable<Guid> deviceIds, CancellationToken ct)
  {
    using var span = Telemetry.Span();
    var deviceIdStrings = deviceIds.ToList();
    var devices = await Devices
      .Where(d => deviceIdStrings.Contains(d.Id))
      .Select(d => d.ToModel())
      .ToListAsync(cancellationToken: ct);
    var interfaces = (await Interfaces.Where(i => deviceIdStrings.Contains(i.DeviceId)).ToListAsync(ct)).GroupBy(i => i.DeviceId)
      .ToDictionary(g => g.Key, g => g.Select(i => i.ToModel()).ToList());
    var interfaceIds = interfaces.Values.SelectMany(g => g.Select(i => i.Id)).ToList();
    var streams = (await Streams.Where(s => interfaceIds.Contains(s.InterfaceId)).ToListAsync(cancellationToken: ct)).GroupBy(s => s.InterfaceId)
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

  public Task<int> GetDeviceCountAsync(Query query, CancellationToken ct)
  {
    using var span = Telemetry.Span();
    return GetDeviceBaseQuery()
      .ApplyFilter(query, _pseudoProperties)
      .CountAsync(ct);
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

  public async Task<QueryResult<InterfaceInfo>> GetInterfaceInfosAsync(Query query, CancellationToken ct)
  {
    using var span = Telemetry.Span();
    var result = (await GetInterfaceBaseQuery()
        .ApplyFilter(query, _pseudoProperties)
        .ApplySorting(query)
        .Skip(query.StartIndex)
        .Take(query.Count)
        .Select(x => x.Interface.MapToInfo(x.Denormalized, x.Device))
        .ToListAsync(ct))
      .ToImmutableList();

    return new(query.StartIndex, result.Count, result);
  }

  public async Task<int> GetInterfaceCountAsync(Query query, CancellationToken ct)
  {
    using var span = Telemetry.Span();
    return await GetInterfaceBaseQuery()
      .ApplyFilter(query, _pseudoProperties)
      .CountAsync(ct);
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

  public async Task<QueryResult<StreamInfo>> GetStreamInfosAsync(Query query, CancellationToken ct)
  {
    using var span = Telemetry.Span();
    var result = (await GetStreamBaseQuery()
        .ApplyFilter(query, _pseudoProperties)
        .ApplySorting(query)
        .Skip(query.StartIndex)
        .Take(query.Count)
        .ToListAsync(ct))
      .Select(x => x.Stream.MapToInfo(x.Interface, x.Device))
      .ToImmutableList();

    return new(query.StartIndex, result.Count, result);
  }

  public async Task<int> GetStreamCountAsync(Query query, CancellationToken ct)
  {
    using var span = Telemetry.Span();
    return await GetStreamBaseQuery()
      .ApplyFilter(query, _pseudoProperties)
      .CountAsync(ct);
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
