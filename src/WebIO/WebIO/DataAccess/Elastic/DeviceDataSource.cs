namespace WebIO.DataAccess.Elastic;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EntityFrameworkCore;
using EntityFrameworkCore.Entities;
using Microsoft.EntityFrameworkCore;
using WebIO.Elastic.Data;
using WebIO.Elastic.Management.Indexing;

public class DeviceDataSource : IDataSource<IndexedDevice, Guid>
{
  private readonly AppDbContext _db;

  public DeviceDataSource(AppDbContext db)
  {
    _db = db;
  }

  public IAsyncEnumerable<IndexedDevice> AllEntitiesAsync(CancellationToken ct)
    => _db.Devices
      .Include(d => d.Properties)
      .AsNoTracking()
      .Select(d => ToIndexedDevice(d, _db))
      .AsAsyncEnumerable();

  public IAsyncEnumerable<IndexedDevice> AllEntitiesWithIds(IEnumerable<Guid> ids, CancellationToken ct)
    => _db.Devices
      .Include(d => d.Properties)
      .AsNoTracking()
      .Where(d => ids.Contains(d.Id))
      .Select(d => ToIndexedDevice(d, _db))
      .AsAsyncEnumerable();

  private static IndexedDevice ToIndexedDevice(DeviceEntity device, AppDbContext db)
    => new()
    {
      Id = device.Id,
      Name = device.Name,
      DeviceType = device.DeviceType,
      Comment = device.Comment ?? string.Empty,
      DeviceProperties = device.Properties.ToDictionary(p => p.Key, p => p.Value ?? string.Empty),
      InterfaceCount = db.Interfaces.Count(i => i.DeviceId == device.Id),
    };
}
